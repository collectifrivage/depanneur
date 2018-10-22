import React from "react";
import {Query, Mutation} from "react-apollo";
import gql from "graphql-tag";

import ProductForm from "../components/product-form";
import Loading from "../components/loading-indicator";

const productQuery = gql`
    query product($id: Int!) {
        product(id: $id) {
            id
            name
            description
            price
            canSubscribe
        }
    }
`;

const updateProductMutation = gql`
    mutation updateProduct($id: Int!, $product: ProductInput!) {
        product(id: $id) {
            update(product: $product) {
                id
                name
                description
                price
                canSubscribe
            }
        }
    }
`;

class EditProduct extends React.Component {

    render() {
        const productId = this.props.match.params.productid;

        return (
            <Query query={productQuery} variables={{id: productId}}>
                {
                    ({loading, error, data}) => {
                        if (loading) return <Loading />;
                        if (error) { /* TODO */ }

                        return (
                            <div>
                                <h1>Modifier le produit: {data.product.name}</h1>

                                <Mutation mutation={updateProductMutation}>
                                    {
                                        (mutate, {loading}) => {
                                            if (loading) return <Loading />
                                            const save = ({name, description, price, canSubscribe}) => mutate({
                                                variables: {
                                                    id: productId,
                                                    product: {name, description, price, canSubscribe}
                                                }
                                            })

                                            return <ProductForm product={data.product}
                                                                onSubmit={p => save(p).then(() => this.goBack())}
                                                                onCancel={() => this.goBack()} />
                                        }
                                    }
                                </Mutation>
                            </div>
                        );
                    }
                }
            </Query>
        );
    }

    goBack() {
        this.props.history.push("/products");
    }

}

export default EditProduct;