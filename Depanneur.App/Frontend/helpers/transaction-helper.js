export const describeTransaction = t => {
    if (t.__typename === "Purchase") {
        let multiplier = "";
        if (t.quantity > 1) multiplier = `${t.quantity}x`;

        return `Achat: ${multiplier}"${t.itemName}" @ ${t.pricePerUnit}`;
    }

    if (t.__typename === "Payment") {
        return "Paiement";
    }

    if (t.__typename === "Adjustment") {
        return `Ajustement - ${t.description}`
    }

    return "";
}