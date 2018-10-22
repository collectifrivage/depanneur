import gql from "graphql-tag";
import React from "react";
import { Query, Mutation } from "react-apollo";

import Amount from "../components/amount";
import { Button, FormField, NumberInput, Radio } from "../components/forms";
import Loading from "../components/loading-indicator";

const subscriptionInfosQuery = gql`
    query SubscriptionInfos($id: Int!) {
        product(id: $id) {
            id
            name
            price
            stats {
                weeklyAverage
            }
            subscription {
                frequency
                quantity
            }
        }
    }
`;

const subscribeMutation = gql`
    mutation Subscribe($id: Int!, $quantity: Int!, $frequency: SubscriptionFrequency!) {
        product(id: $id) {
            subscribe(quantity: $quantity, frequency: $frequency) {
                product {
                    id
                    isSubscribed
                }
            }
        }
    }
`;

const unsubscribeMutation = gql`
    mutation Unsubscribe($id: Int!) {
        product(id: $id) {
            unsubscribe {
                product {
                    id
                    isSubscribed
                }
            }
        }
    }
`;

export default class Subscription extends React.Component {
    constructor(props) {
        super(props);
        this.productId = props.match.params.productid;
    }

    render() {
        return (
            <Query query={subscriptionInfosQuery} variables={{id: this.productId}} fetchPolicy="cache-and-network">
                {
                    ({loading, data}) => {
                        if (loading) return <Loading />

                        return (
                            <div>
                                {
                                    data.product.subscription
                                        ? <h1>Modifier l'abonnement: {data.product.name}</h1>
                                        : <h1>S'abonner au produit: {data.product.name}</h1>
                                }

                                <p>Si votre consommation de ce produit est plutôt constante, vous pouvez configurer un achat récurrent. Fini les oublis!</p>
                
                                {
                                    data.product.stats.weeklyAverage
                                        ? <p>Selon votre historique, vous achetez ce produit en moyenne {data.product.stats.weeklyAverage}x par semaine.</p>
                                        : null
                                }

                                <EditForm sub={data.product.subscription} 
                                          productId={data.product.id}
                                          price={data.product.price}
                                          back={() => this.back()} />
                            </div>
                        );
                    }
                }
            </Query>
        )
    }


    back() {
        this.props.history.push("/");
    }
}

class EditForm extends React.Component {
    constructor(props) {
        super(props);

        if (props.sub) {
            this.state = {
                new: false,
                quantity: props.sub.quantity,
                frequency: props.sub.frequency
            };
        } else {
            this.state = {
                new: true,
                quantity: 0,
                frequency: "WEEKLY"
            };
        }
    }

    render() {
        const {new: isNew, quantity, frequency} = this.state;
        const {back, productId} = this.props;

        return (
            <div>
                <FormField label="Quantité">
                    <NumberInput value={quantity} style={{width: 100}} decimals={0} onValueChanged={v => this.setState({quantity:v})} />
                </FormField>
                <FormField label="Fréquence">
                    <div>
                        <Radio label="Quotidien" checked={frequency === "DAILY"} onChecked={_ => this.setState({frequency: "DAILY"})} />
                        <Radio label="Hebdomadaire" checked={frequency === "WEEKLY"} onChecked={_ => this.setState({frequency: "WEEKLY"})} />
                    </div>
                </FormField>
                
                <p>{this.getSummary()}</p>

                <Mutation mutation={subscribeMutation} variables={{id: productId, quantity: quantity, frequency: frequency}}>
                    {(mutate) => <Button disabled={!this.isValid()} onClick={() => mutate().then(back)}>Enregistrer</Button>}
                </Mutation>

                {isNew 
                    ? null 
                    : <Mutation mutation={unsubscribeMutation} variables={{id: productId}}>
                        {(mutate) => <Button onClick={() => this.confirmUnsub().then(mutate).then(back)}>Supprimer</Button>}
                      </Mutation>
                }

                <Button onClick={() => back()}>Annuler</Button>
            </div>
        );
    }

    isValid() {
        return this.state.quantity > 0;
    }

    getSummary() {
        if (!this.isValid()) return null;

        const total = this.state.quantity * this.props.price;

        switch (this.state.frequency) {
            case "DAILY":
                return <span>Un montant total de <Amount amount={total} /> vous sera facturé à chaque jour de semaine à 16h (<Amount amount={total * 5} /> par semaine).</span>;
            case "WEEKLY":
                return <span>Un montant total de <Amount amount={total} /> vous sera facturé à chaque vendredi à 17h.</span>;
            default:
                return null;
        }
    }

    confirmUnsub() {
        if (confirm("Êtes-vous certain de vouloir supprimer cet abonnement ?")) return Promise.resolve();
        return Promise.reject();
    }
}