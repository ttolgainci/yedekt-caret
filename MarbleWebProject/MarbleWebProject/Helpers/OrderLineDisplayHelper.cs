using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class OrderLineDisplayHelper
{
  private static decimal RoundMoney(decimal value) =>
      Math.Round(value, 2, MidpointRounding.AwayFromZero);

  /// <summary>Mağazada görünen KDV dahil satır tutarı.</summary>
  public static decimal GetGrossLineTotal(ShopOrderLineModel line)
  {
    var qty = line.Quantity <= 0 ? 1 : line.Quantity;
    if (line.UnitPrice > 0)
      return RoundMoney(line.UnitPrice * qty);

    if (line.TotalAmount > 0)
      return RoundMoney(line.TotalAmount);

    return line.VatAmount > 0
        ? RoundMoney(line.LineTotal + line.VatAmount)
        : RoundMoney(line.LineTotal);
  }

  public static decimal GetGrossUnitPrice(ShopOrderLineModel line)
  {
    var qty = line.Quantity <= 0 ? 1 : line.Quantity;
    return RoundMoney(GetGrossLineTotal(line) / qty);
  }
}
