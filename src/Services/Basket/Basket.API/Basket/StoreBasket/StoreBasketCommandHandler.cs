using Discount.Grpc;

namespace Basket.API.Basket.StoreBasket;

public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;

public record StoreBasketResult(string UserName);

public class StoreBasketCommandHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProto) 
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        await DeductDiscount(command.Cart, cancellationToken);

        // Store Basket in DP (Marten and Redis Cache)
        await repository.StoreBasket(command.Cart, cancellationToken);

        return new StoreBasketResult(command.Cart.UserName);
    }

    private async Task DeductDiscount(ShoppingCart shoppingCart, CancellationToken cancellationToken)
    {
        // Communicate with Discount.Grpc and calc latest prices
        foreach (var shoppingCartItem in shoppingCart.Items)
        {
            var coupon = await discountProto.GetDiscountAsync(new GetDiscountRequest
                { ProductName = shoppingCartItem.ProductName }, cancellationToken: cancellationToken);

            shoppingCartItem.Price -= coupon.Amount;
        }

    }
}