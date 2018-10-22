import React from "react";
import { FormField, NumberInput, TextInput, Checkbox, Button } from "../components/forms";
import ErrorMessage from "../components/error-message";

class ProductForm extends React.Component {
    constructor(props) {
        super(props);

        var p = props.product || {};
        
        this.state = {
            error: null,
            name: p.name,
            description: p.description,
            price: p.price,
            canSubscribe: !!p.canSubscribe
        };
    }

    render() {
        const { error, name, description, price, canSubscribe } = this.state;

        return (
            <form onSubmit={this.onSubmit}>
                <FormField label="Nom du produit">
                    <TextInput value={name} style={{ width: 400, maxWidth: "100%" }} onValueChanged={v => this.setState({ name: v })} />
                </FormField>
                <FormField label="Description">
                    <TextInput value={description} style={{ width: 400, maxWidth: "100%" }} onValueChanged={v => this.setState({ description: v })} />
                </FormField>
                <FormField label="Prix ($)">
                    <NumberInput value={price} style={{ width: 100 }} decimals={2} onValueChanged={v => this.setState({ price: v })} />
                </FormField>
                <FormField help="Si cette option est cochée, un bouton permettant de s'abonner au produit sera affiché dans le catalogue.">
                    <Checkbox label="Permettre l'abonnement" checked={canSubscribe} onChecked={v => this.setState({ canSubscribe: v })} />
                </FormField>

                {error ? <ErrorMessage>{error}</ErrorMessage> : null}

                <Button type="submit">Enregistrer</Button>
                <Button onClick={() => this.props.onCancel()}>Annuler</Button>
            </form>
        )
    };

    onSubmit = ev => {
        ev.preventDefault();

        const { name, description, price, canSubscribe } = this.state;

        if (this.validate()) {
            this.props.onSubmit({ name, description, price, canSubscribe });
        }
    }

    validate() {
        const { name, price } = this.state;
        let err = null;

        if (!name || name.trim() === "") {
            err = "Le nom du produit est requis.";
        }
        else if (!price || price <= 0) {
            err = "Le prix doit être supérieur à zéro."
        }

        this.setState({
            error: err
        });

        return err === null;
    }
};

export default ProductForm;