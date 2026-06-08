namespace MarbleWebProject.Models;



public sealed class StoreBankAccountItemModel

{

    public int ID { get; set; }

    public string BankName { get; set; } = "";

    public string? AccountHolder { get; set; }

    public string Iban { get; set; } = "";

    public string? Logo { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

}



public sealed class StoreBankTransferInfoModel

{

    public string StoreName { get; set; } = "";

    public string? Instructions { get; set; }

    public List<StoreBankAccountItemModel> Accounts { get; set; } = new();



    // Geriye dönük uyumluluk (tek hesap)

    public string? BankName => Accounts.FirstOrDefault()?.BankName;

    public string? BankAccountHolder => Accounts.FirstOrDefault()?.AccountHolder;

    public string? BankIban => Accounts.FirstOrDefault()?.Iban;

}



public sealed class CheckoutConfirmationViewModel

{

    public int OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public decimal GrandTotal { get; set; }

    public string CurrencyCode { get; set; } = "";

    public string PaymentMethod { get; set; } = "";

    public ShopOrderDetailModel? Order { get; set; }

    public StoreBankTransferInfoModel? BankInfo { get; set; }

}

