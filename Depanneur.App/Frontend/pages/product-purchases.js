import gql from "graphql-tag";
import React from "react";
import { Query } from "react-apollo";

import Amount from "../components/amount";
import Date from "../components/date";
import { SubscriptionIcon } from "../components/icons";
import Loading from "../components/loading-indicator";
import Paging from "../components/paging";
import { Cell, HeaderCell, Row, Table } from "../components/tables";


const productPurchasesQuery = gql`
  query ProductPurchases($id: Int!, $page: Int) {
    product(id: $id) {
      id
      name
      purchases(page: $page) {
        currentPage
        pageCount
        items {
          id
          timestamp
          quantity
          pricePerUnit
          amount
          wasFromSubscription
          user {
            id
            name
          }
        }
      }
    }
  }  
`;

class ProductPurchases extends React.Component {

    constructor() {
        super();
        this.state = {
            currentPage: 1
        };
    }

    render() {
        return (
            <Query query={productPurchasesQuery} variables={{id: this.props.match.params.productid, page: this.state.currentPage}} fetchPolicy="cache-and-network">
                {
                    ({error, data}) => {
                        if (error && (!data || !data.product || data.product.purchases.currentPage != this.state.currentPage)) { return <div>Erreur</div> }
                        if (!data.product || data.product.purchases.currentPage != this.state.currentPage) return <Loading />;

                        return (
                            <React.Fragment>
                                <h1>Historique d'achat pour {data.product.name}</h1>
                                {this.renderPurchases(data.product.purchases.items)}
                                {
                                    data.product.purchases.pageCount > 1
                                        ? <Paging style={{marginTop: 20}} 
                                                  current={this.state.currentPage} 
                                                  count={data.product.purchases.pageCount} 
                                                  onPageChanged={p => this.setState({currentPage: p})} />
                                        : null
                                }
                            </React.Fragment>
                        );
                    }
                }
            </Query>
        )
    }

    renderPurchases(purchases) {
        let i = 0;
        const header = [
            <HeaderCell key={i++}>Date et heure</HeaderCell>,
            <HeaderCell key={i++}>Acheteur</HeaderCell>,
            <HeaderCell key={i++} style={styles.right}>Montant payé</HeaderCell>
        ];

        return (
            <Table header={header}>
                {purchases.map(p => this.renderPurchase(p))}
            </Table>
        );
    }

    renderPurchase(p) {
        return (
            <Row key={p.id}>
                <Cell><Date date={p.timestamp} /></Cell>
                <Cell>{p.user.name}</Cell>
                <Cell style={styles.right}>
                    {
                        p.wasFromSubscription
                            ? <SubscriptionIcon style={{verticalAlign:"middle", marginRight: 5}} />
                            : null
                    }
                    {
                        p.quantity > 1
                            ? <span>{p.quantity}x <Amount amount={p.pricePerUnit} /> = </span>
                            : null
                    }
                    <Amount amount={p.amount} />
                </Cell>
            </Row>
        );
    }

}

const styles = {
    right: {
        textAlign: "right"
    }
}

export default ProductPurchases;