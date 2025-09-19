using System.Threading.Tasks;
using cybersoft_final_project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD392_backend.Infrastructure.Services.S3Service;
using SWD392_backend.Infrastructure.Services.UploadService;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly IUploadService _uploadService;

        public UploadController(IS3Service s3Service, IUploadService uploadService)
        {
            _s3Service = s3Service;
            _uploadService = uploadService;
        }

        /// <summary>
        /// Tạo các URL tạm thời (presigned URLs) để tải lên nhiều hình ảnh sản phẩm.
        /// </summary>
        /// <param name="request">Đối tượng chứa thông tin yêu cầu tải lên hình ảnh, bao gồm CategoryId, ProductSlug, ProductId, SupplierId và ContentTypes.</param>
        /// <returns>
        /// - 200 OK: Trả về UploadMultipleProductImgsResponse chứa danh sách các URL tạm thời và thông tin hình ảnh.
        /// - 400 Bad Request: Nếu yêu cầu không hợp lệ (thiếu thông tin hoặc ContentTypes rỗng).
        /// - 500 Internal Server Error: Nếu xảy ra lỗi server (ví dụ: không lấy được slug danh mục hoặc lỗi dịch vụ S3).
        /// </returns>
        /// <remarks>
        /// Tạo presigned URLs cho hình ảnh sản phẩm dựa trên danh mục, sản phẩm và nhà cung cấp. Mỗi URL được tạo kèm key và liên kết CDN (hình đầu tiên sẽ là hình chính cho product).
        /// </remarks>
        [HttpPost("upload-images")]
        public async Task<ActionResult<UploadProductImgResponse>> GetPresignedUrl([FromBody] UploadProductImgsRequest request, bool isSupplierId = false)
        {

            var upload = await _uploadService.UploadMultipleImage(request, isSupplierId);

            if (upload == null)
                return BadRequest(HTTPResponse<object>.Response(400, "Có lỗi xảy ra", null));

            return Ok(HTTPResponse<object>.Response(200, "Successfully", upload));
        }

        /// <summary>
        /// Gửi lại link các hình ảnh nhận được từ endpoint upload-images
        /// </summary>
        /// <param name="id">ID của sản phẩm (kiểu số nguyên).</param>
        /// <param name="imageUrl">Danh sách URL của các hình ảnh đã tải lên.</param>
        /// <returns>
        /// - 204 No Content: Nếu xác nhận và liên kết hình ảnh thành công.
        /// - 400 Bad Request: Nếu ID không hợp lệ, danh sách URL rỗng, hoặc xác nhận thất bại.
        /// - 500 Internal Server Error: Nếu xảy ra lỗi server.
        /// </returns>
        /// <remarks>
        /// Xác nhận các hình ảnh đã tải lên, lưu vào cơ sở dữ liệu, và cập nhật sản phẩm trong Elasticsearch. (link đầu tiên sẽ là hình chính cho product)
        /// </remarks>
        [HttpPost("{id:int}/confirm-upload")]
        public async Task<IActionResult> ConfirmUploadImage(int id, [FromBody] List<string> imageUrl)
        {
            var confirm = await _uploadService.ConfirmUploadImage(id, imageUrl);

            if (!confirm)
                return BadRequest(HTTPResponse<object>.Response(400, "Upload ảnh thất bại", confirm));
            else 
                return Ok(HTTPResponse<object>.Response(200,"Upload ảnh thành công", confirm));
        }

        [HttpPost("{id:int}/confirm-upload-supplier")]
        public async Task<IActionResult> ConfirmUploadSupplierImage(int id, [FromBody] List<string> imageUrl)
        {
            var confirm = await _uploadService.ConfirmUploadSupplierImage(id, imageUrl);

            if (!confirm)
            {
                return BadRequest(HTTPResponse<object>.Response(400, "Upload ảnh thất bại", confirm));
            }
            else
            {
                return Ok(HTTPResponse<object>.Response(200, "Upload ảnh thành công", confirm));
            }
        }
    }
}
