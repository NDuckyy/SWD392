using System.Linq;
using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.ElasticDocs;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ElasticSearchService
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly IMapper _mapper;
        public ElasticSearchService(IMapper mapper)
        {
            var uri = Environment.GetEnvironmentVariable("ELS_URI");
            var username = Environment.GetEnvironmentVariable("ELS_USERNAME");
            var password = Environment.GetEnvironmentVariable("ELS_PASSWORD");

            if (string.IsNullOrEmpty(uri))
                throw new Exception("env not set!");

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .Authentication(new BasicAuthentication(username, password));

            // Create client
            _client = new ElasticsearchClient(settings);
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductResponse>> SearchAsync(
            string q = "",
            int? categoryId = null,
            int page = 1,
            int size = 10,
            string sortBy = "createdAt",
            string sortOrder = "desc"
        )
        {
            List<ProductResponse> response;

            var order = sortOrder.ToLower() == "asc" ? SortOrder.Asc : SortOrder.Desc;
            var sortField = sortBy.ToLower() == "name" ? "name.keyword" : sortBy;

            // Filter category
            var filters = new List<Query>();

            if (categoryId != null)
                filters.Add(new TermQuery("categoriesId")
                {
                    Value = categoryId.Value
                });

            // Filter is active
            filters.Add(new TermQuery("isActive") { Value = true });

            // Create query
            Query query;
            if (string.IsNullOrEmpty(q))
                query = new BoolQuery
                {
                    Filter = filters
                };
            else
            {
                var shouldQuery = new List<Query>
                {
                    new MultiMatchQuery
                    {
                        Query = q,
                        Fields = new[] {
                            "name.vi^3",
                            "slug^2",
                        },
                        Operator = Operator.And,
                        Fuzziness = new Fuzziness("AUTO"),
                        MinimumShouldMatch = "75%"
                    },
                    new MatchPhrasePrefixQuery("name.autocomplete")
                    {
                        Query = q,
                        Boost = 4
                    }
                };

                query = new BoolQuery
                {
                    Should = shouldQuery,
                    Filter = filters,
                    MinimumShouldMatch = 1
                };
            }

            var searchResponse = await _client.SearchAsync<ProductResponse>(s => s
                    .Indices("products")
                    .Sort(sort => sort.Field(sortField, new FieldSort { Order = order }))
                    .From((page - 1) * size)
                    .Size(size)
                    .Query(query)
                );

            response = searchResponse.Hits.Select(h => h.Source).ToList();
            return new PagedResult<ProductResponse>
            {
                Items = response,
                TotalItems = (int)searchResponse.Total,
                Page = page,
                PageSize = size,               
            };

        }

        public async Task IndexProductAsync(product product)
        {
            var doc = _mapper.Map<ProductElasticDoc>(product);
            var id = product.Id.ToString();

            await _client.IndexAsync(doc, x => x.Index("products").Id(id));
        }

        public async Task UpdateProductAsync(product product)
        {
            Console.WriteLine($"---------------------------{product.IsActive} || {product.Id}");

            var doc = _mapper.Map<ProductElasticDoc>(product);
            var id = product.Id.ToString();

            await _client.UpdateAsync<ProductElasticDoc, ProductElasticDoc>("products", id, u => u.Doc(doc));
        }

        public async Task RemoveProductAsync(int id)
        {
            var docId = id.ToString();

            await _client.DeleteAsync<ProductElasticDoc>("products", docId);
        }
    }
}
