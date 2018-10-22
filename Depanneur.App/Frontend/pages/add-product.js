import React from "react";
import ProductForm from "../components/product-form";
import { Mutation } from "react-apollo";
import gql from "graphql-tag";

const addProductMutation = gql`
    mutation AddProduct($product: ProductInput) {
        addProduct(product: $product) { id }
    }
`;

class AddProduct extends React.Component {

    constructor() {
        super();
    }

    render() {
        return (
            <div>
                <h1>Ajouter un produit</h1>

                <Mutation mutation={addProductMutation}>
                    {
                        (mutate) => <ProductForm onSubmit={p => mutate({variables:{product: p}}).then(() => this.back())} onCancel={() => this.back()} />
                    }
                </Mutation>
            </div>
        )
    }

    back() {
        this.props.history.push("/products");
    }

}

export default AddProduct;