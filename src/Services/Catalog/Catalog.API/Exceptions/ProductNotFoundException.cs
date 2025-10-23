namespace Catalog.API.Exceptions;

internal class ProductNotFoundException : Exception
{
    public ProductNotFoundException() 
        : base($"Product not found.")
    {
        
    }
}