import React from "react";
import Radium from "radium";
import store from "store";
import { Query, Mutation, withApollo } from "react-apollo";
import gql from "graphql-tag";

import Loading from "../components/loading-indicator";
import BuyButton from "../components/buy-button";
import Amount from "../components/amount";
import OptionGroup from "../components/option-group";
import CircleButton from "../components/circle-button";
import events from "../events";
import { SubscriptionIcon } from "../components/icons";
import { Checkbox } from "../components/forms";

const catalogQuery = gql`
  query catalog($sort: ProductSortOption!, $stats: Boolean!) {
    products(sort: $sort) {
      id
      name
      description
      price
      canSubscribe
      isSubscribed
      stats @include(if: $stats) {
        total
        month
      }
    }
  }
`;

const purchaseMutation = gql`
  mutation Purchase($productId: Int!) {
    purchase(productId: $productId) {
      id
      user {
        id
        balance
      }
      product {
        id
        stats {
          total
          month
        }
      }
    }
  }
`;

const cancelPurchasesMutation = gql`
  mutation CancelPurchases($purchases: [Int!]!) {
    cancelPurchases(purchaseIds: $purchases) {
      user {
        id
        balance
      }
      products {
        id
        stats {
          total
          month
        }
      }
    }
  }
`;

class Home extends React.Component {
  constructor() {
    super();

    this.sortOptions = [
      { key: "alpha", label: "Alphabétique" },
      { key: "usage", label: "Selon l'utilisation" }
    ];

    this.state = {
      sortOption: store.get("dep.catalog.sort") || "alpha",
      showStats: store.get("dep.catalog.showStats") !== false,
      groupSubscriptions: store.get("dep.catalog.groupSubscriptions") !== false
    };

    this.recentPurchases = [];
  }

  render() {
    return (
      <Query
        query={catalogQuery}
        variables={{ sort: this.state.sortOption, stats: this.state.showStats }}
        pollInterval={10000}
        fetchPolicy="cache-and-network"
      >
        {({ loading, error, data }) => {
          if (loading && !data.products)
            return (
              <div style={{ marginTop: 10 }}>
                <Loading />
              </div>
            );
          if (error) {
            /* TODO */
          }

          return this.renderCatalog(data.products);
        }}
      </Query>
    );
  }

  renderCatalog(products) {

    return (
      <div>
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            flexWrap: "wrap"
          }}
        >
          <OptionGroup
            label="Tri des produits :"
            style={{ marginTop: 10 }}
            options={this.sortOptions}
            selectedOption={this.state.sortOption}
            onOptionSelected={o => this.setSortOption(o)}
          />

          <div style={{marginTop: 5}}>
            <Checkbox
              label="Afficher les statistiques d'achat"
              checked={this.state.showStats}
              onChecked={c => this.setStatsVisible(c)}
            />
            {
              products.filter(x => x.isSubscribed).length > 0 &&
              <React.Fragment>
                <br/>
                <Checkbox
                  label="Regrouper les abonnements"
                  checked={this.state.groupSubscriptions}
                  onChecked={c => this.setGroupSubscriptions(c)}
                />
              </React.Fragment>
            }
          </div>
        </div>

        {
          this.state.groupSubscriptions 
            ? this.renderGroupedSubscriptions(products)
            : this.renderProductList(products)
        }
        
      </div>
    );
  }

  renderGroupedSubscriptions(products) {
    const subscribed = products.filter(x => x.isSubscribed);
    const notSubscribed = products.filter(x => !x.isSubscribed);

    return (
      <React.Fragment>
        {
          subscribed.length > 0 &&
          <React.Fragment>
            <p>Produits abonnés</p>
            <p style={{color:"#999", fontSize: "75%"}}>Ces produits vous sont automatiquement facturés selon les paramètres choisis. Tout achat effectué ici sera facturé en surplus à votre abonnement.</p>
            {this.renderProductList(subscribed)}
          </React.Fragment>
        }

        {
          subscribed.length > 0 && notSubscribed.length > 0 &&
          <React.Fragment><hr/><p>Autres produits</p></React.Fragment>
        }

        {this.renderProductList(notSubscribed)}
      </React.Fragment>
    );
  }

  renderProductList(products) {
    return (
      <table
        style={{
          width: "100%",
          borderCollapse: "collapse",
          margin: "10px 0"
        }}
      >
        <tbody>
          {products.map((p, i) => this.renderCatalogRow(p, i === 0))}
        </tbody>
      </table>
    )
  }

  renderCatalogRow(product, isFirst) {
    const cellStyle = {
      borderTop: isFirst ? "none" : "1px solid #CCC",
      padding: 10
    };

    return (
      <tr key={product.id}>
        <td style={cellStyle}>
          {product.name}
          <span style={style.description}>
            {product.description}
            {product.stats
              ? this.renderStats(product.stats.month, product.stats.total)
              : null}
          </span>
        </td>
        <td style={[cellStyle, { textAlign: "right" }]}>
          <Amount amount={product.price} />
        </td>
        <td style={[cellStyle, { width: 80, padding: "10px 0" }]}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            {
              (!product.isSubscribed || this.state.groupSubscriptions) &&
              <Mutation
                mutation={purchaseMutation}
                variables={{ productId: product.id }}
              >
                {mutate => (
                  <BuyButton onClick={() => this.onBuy(mutate, product)}>
                    +1
                  </BuyButton>
                )}
              </Mutation>
            }
            {product.canSubscribe || product.isSubscribed ? (
              <SubscribeButton
                subscribed={product.isSubscribed}
                onClick={() =>
                  this.props.history.push(`/subscription/${product.id}`)
                }
              />
            ) : null}
          </div>
        </td>
      </tr>
    );
  }

  renderStats(month, total) {
    if (total === 0) return null;

    if (month > 0) {
      return (
        <span style={style.stats}>
          Acheté <span style={style.nowrap}>{total}x au total,</span>{" "}
          <span style={style.nowrap}>{month}x ce mois-ci.</span>
        </span>
      );
    } else {
      return (
        <span style={style.stats}>
          Acheté <span style={style.nowrap}>{total}x au total.</span>
        </span>
      );
    }
  }

  componentWillUnmount() {
    if (this.batchTimeout) {
      clearTimeout(this.batchTimeout);
    }
  }

  onBuy(mutate, p) {
    const BATCH_PURCHASE_TIMEOUT = 10000;

    return mutate().then(({ data }) => {
      this.recentPurchases.push({
        product: p,
        purchaseId: data.purchase.id
      });

      events.trigger("show-notification", {
        message: this.formatRecentPurchases(),
        actions: [
          {
            label: "Annuler",
            onClick: () => {
              this.props.client.mutate({
                mutation: cancelPurchasesMutation,
                variables: {
                  purchases: this.recentPurchases.map(p => p.purchaseId)
                }
              });

              clearTimeout(this.batchTimeout);
              this.recentPurchases = [];
            }
          }
        ],
        timeout: BATCH_PURCHASE_TIMEOUT
      });

      if (this.batchTimeout) {
        clearTimeout(this.batchTimeout);
      }

      this.batchTimeout = setTimeout(() => {
        this.recentPurchases = [];
      }, BATCH_PURCHASE_TIMEOUT);
    });
  }

  formatRecentPurchases() {
    let total = 0;
    let description = "";

    const counts = {};

    for (let p of this.recentPurchases) {
      total += p.product.price;

      if (!counts[p.product.name]) {
        counts[p.product.name] = 1;
      } else {
        counts[p.product.name]++;
      }
    }

    for (let key of Object.keys(counts).sort()) {
      const count = counts[key];

      if (description !== "") {
        description += ", ";
      }

      if (count > 1) {
        description += `${count}x ${key}`;
      } else {
        description += key;
      }
    }

    return (
      <span>
        Achat confirmé: {description} (<Amount amount={total} />)
      </span>
    );
  }

  setSortOption(option) {
    store.set("dep.catalog.sort", option);

    this.setState({
      sortOption: option
    });
  }

  setStatsVisible(visible) {
    store.set("dep.catalog.showStats", visible);

    this.setState({
      showStats: visible
    });
  }

  setGroupSubscriptions(group) {
    store.set("dep.catalog.groupSubscriptions", group);

    this.setState({
      groupSubscriptions: group
    });
  }

  sortProducts(products, option) {
    const copy = products.slice();
    copy.sort(function(a, b) {
      function compareNames() {
        if (a.name < b.name) return -1;
        if (a.name > b.name) return 1;
        return 0;
      }

      switch (option) {
        case "usage":
          return b.total - a.total || compareNames();
        default:
          return compareNames();
      }
    });

    return copy;
  }
}

function SubscribeButton(props) {
  var color = props.subscribed ? "#CFC" : "#DED";
  return (
    <CircleButton color={color} onClick={props.onClick}>
      <SubscriptionIcon style={{ opacity: 0.8 }} />
    </CircleButton>
  );
}

const style = {
  description: {
    display: "block",
    marginTop: 3,
    fontSize: "75%",
    color: "#999"
  },
  stats: {
    display: "block",
    marginTop: 3,
    color: "#ccc"
  },
  nowrap: {
    whiteSpace: "nowrap"
  }
};

export default withApollo(Radium(Home));
