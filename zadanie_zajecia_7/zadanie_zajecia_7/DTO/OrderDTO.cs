namespace zadanie_zajecia_7.DTO;

public record OrderDTO(
    int IdOrder,
    int IdProduct,
    int Amount,
    DateTime CreatedAt
    );