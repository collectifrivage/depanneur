import gql from "graphql-tag";
import React from "react";
import { Mutation, Query } from "react-apollo";
import { Link } from "react-router-dom";

import Amount from "../components/amount";
import ErrorMessage from "../components/error-message";
import { Button, FormField, NumberInput } from "../components/forms";
import Loading from "../components/loading-indicator";


const getUserBalanceQuery = gql`
    query GetUserBalance($id: String!) {
        user(id: $id) {
            name
            balance
        }
    }
`;

const recordPaymentMutation = gql`
    mutation RecordPayment($userId: String!, $amount: Decimal!) {
        user(id: $userId) {
            recordPayment(amount: $amount) {
                user {
                    id
                    balance
                }
            }
        }
    }
`;

export default class Payment extends React.Component {

    constructor() {
        super();

        this.state = {
            user: null
        };
    }

    render() {
        const { userid } = this.props.match.params;

        return (
            <div>
                <h1>Enregistrer un paiement</h1>

                <Query query={getUserBalanceQuery} variables={{ id: userid }}>
                    {
                        ({ loading, data }) => {
                            if (loading) return <Loading />

                            return (
                                <div>
                                    <UserInfos user={data.user} />
                                    <Mutation mutation={recordPaymentMutation}>
                                        {
                                            (mutate, { loading }) => {
                                                if (loading) return <Loading />
                                                return <PaymentForm onSubmit={({ amount }) => mutate({ variables: { userId: userid, amount } }).then(() => this.goBack())} />
                                            }
                                        }
                                    </Mutation>
                                </div>
                            );
                        }
                    }
                </Query>
            </div>
        );
    }

    goBack() {
        this.props.history.push("/balances");
    }

}

const UserInfos = props => {
    return (
        <p>
            Pour {props.user.name}<br />
            Solde actuel: <Amount amount={props.user.balance} />
        </p>
    );
}

class PaymentForm extends React.Component {
    constructor() {
        super();

        this.state = {
            amount: 0
        };
    }

    render() {
        return (
            <form onSubmit={ev => { this.onSubmit(); ev.preventDefault(); }}>
                <FormField label="Montant du paiement ($)">
                    <NumberInput value={this.state.amount} decimals={2} onValueChanged={value => this.setState({ amount: value })} autoFocus />
                </FormField>

                {this.state.error ? <ErrorMessage>{this.state.error}</ErrorMessage> : null}

                <Button type="submit">Enregistrer</Button>
                <Link to="/balances"><Button>Annuler</Button></Link>
            </form>
        );
    }

    onSubmit = () => {
        if (this.state.amount <= 0) {
            this.setState({
                error: "Le montant du paiement doit être supérieur à zéro"
            });
            return;
        }

        this.setState({ error: null });
        this.props.onSubmit({ amount: this.state.amount });
    }
}