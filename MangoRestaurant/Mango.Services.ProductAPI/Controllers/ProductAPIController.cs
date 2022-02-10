using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dtos;
using Mango.Services.ProductAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/products")]
    public class ProductAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private readonly IRepository<Product, ProductDto> _repository;

        public ProductAPIController(IRepository<Product, ProductDto> repository) 
        {
            _repository = repository;
            this._response = new ResponseDto();
        }
  
        [HttpGet]
        public async Task<object> Get()
        {
            try
            {
                IEnumerable<ProductDto> products = await _repository.GetAsync();
                _response.Result = products;

            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
        
        [HttpGet]
        [Route("{id}")]
        public async Task<object> Get(int id)
        {
            try
            {
                ProductDto product = await _repository.GetAsync(id);
                _response.Result = product;

            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
        [Authorize]
        [HttpPost]        
        public async Task<object> Post([FromBody] ProductDto product)
        {
            try
            {
                ProductDto model = await _repository.Create(product);
                _response.Result = model;

            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
        [Authorize]
        [HttpPut]
        public async Task<object> Put([FromBody] ProductDto product)
        {
            try
            {
                ProductDto model = await _repository.Update(product);
                _response.Result = model;

            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
   
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<object> Delete(int id)
        {
            try
            {
                var IsSuccess = await _repository.Delete(id);
                _response.Result = IsSuccess;

            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
    }
}
