using BuildingBlocks.Exceptions;

namespace Catalog.API.Exceptions;

internal class ProductNotFoundException(Guid id) : NotFoundException("Product", id);