import React from "react";
import { Link } from "react-router-dom";
import store from "store";
import { Query, withApollo } from "react-apollo";
import gql from "graphql-tag";

import Loading from "../components/loading-indicator";
import ProductList from "../components/product-list";
import { Checkbox } from "../components/forms";

const productsQuery = gql`
  query products($deleted: Boolean!) {
    products(includeDeleted: $deleted) {
      id
      name
      price
      isDeleted
    }
  }
`;

const deleteProductMutation = gql`
  mutation DeleteProduct($id: Int!) {
    product(id: $id) {
      delete {
        id
        isDeleted
      }
    }
  }
`;

const restoreProductMutation = gql`
  mutation DeleteProduct($id: Int!) {
    product(id: $id) {
      restore {
        id
        isDeleted
      }
    }
  }
`;

class Products extends React.Component {
  constructor() {
    super();
    this.state = {
      hideDeleted: store.get("dep.products.hideDeleted") === true
    };
  }

  render() {
    return (
      <div>
        <h1>Produits</h1>
        <p>
          <Link to="/products/add">Ajouter un produit</Link>
        </p>

        <Checkbox
          label="Masquer les produits supprimés"
          checked={this.state.hideDeleted}
          onChecked={c => this.setHideDeleted(c)}
        />

        <Query
          query={productsQuery}
          variables={{ deleted: !this.state.hideDeleted }}
          pollInterval={10000}
          fetchPolicy="cache-and-network"
        >
          {({ loading, error, data }) => {
            if (loading && !data.products) return <Loading />;
            if (error) {
              /* TODO */
            }

            return (
              <ProductList
                products={data.products}
                toggleDelete={p => this.toggleDelete(p)}
              />
            );
          }}
        </Query>
      </div>
    );
  }

  toggleDelete(product) {
    var mutation = product.isDeleted
      ? restoreProductMutation
      : deleteProductMutation;
    return this.props.client.mutate({
      mutation,
      variables: { id: product.id }
    });
  }

  setHideDeleted(hide) {
    store.set("dep.products.hideDeleted", hide);
    this.setState({
      hideDeleted: hide
    });
  }
}

export default withApollo(Products);
