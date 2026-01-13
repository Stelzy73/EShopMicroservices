namespace Basket.API.Basket.StoreBasket;

public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketCommandValidator()
    {
        RuleFor(command => command.Cart).NotNull().WithMessage("Cart is required");
        RuleFor(command => command.Cart.UserName).NotEmpty().WithMessage("UserName is required");
    }
}