namespace zadanie_zajecia_7.DTO;

public record CreateProduct_WarehouseDTO(
    int IdProduct,
    int IdWarehouse,
    int Amount,
    DateTime CreatedAt
    );