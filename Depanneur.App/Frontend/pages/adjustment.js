import gql from "graphql-tag";
import React from "react";
import { Mutation, Query } from "react-apollo";
import { Link } from "react-router-dom";

import Amount from "../components/amount";
import ErrorMessage from "../components/error-message";
import { Button, FormField, NumberInput, TextInput } from "../components/forms";
import Loading from "../components/loading-indicator";

const getUserBalanceQuery = gql`
    query GetUserBalance($id: String!) {
        user(id: $id) {
            name
            balance
        }
    }
`;

const recordAdjustmentMutation = gql`
    mutation RecordAdjustment($userId: String!, $adjustment: AdjustmentInput) {
        user(id: $userId) {
            recordAdjustment(adjustment: $adjustment) {
                user {
                    id
                    balance
                }
            }
        }
    }
`;

class Adjustment extends React.Component {

    render() {
        const { userid } = this.props.match.params;

        return (
            <div>
                <h1>Ajustement de solde</h1>

                <Query query={getUserBalanceQuery} variables={{ id: userid }} fetchPolicy="cache-and-network">
                    {
                        ({ loading, data }) => {
                            if (loading) return <Loading />;

                            return (
                                <div>
                                    <UserInfos user={data.user} />

                                    <Mutation mutation={recordAdjustmentMutation}>
                                        {
                                            (mutate, {loading}) => {
                                                if (loading) return <Loading />;

                                                return (
                                                    <AdjustmentForm previousBalance={data.user.balance}
                                                                    onSubmit={adj => mutate({ variables: { userId: userid, adjustment: adj } }).then(() => this.goBack())} />
                                                );
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

class AdjustmentForm extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            amount: 0,
            newBalance: props.previousBalance,
            description: ""
        };
    }

    render() {
        return (
            <form onSubmit={ev => { this.onSubmit(); ev.preventDefault(); }}>
                <FormField label="Montant de l'ajustement ($)" help="Entrez un montant négatif pour inscrire un crédit">
                    <NumberInput value={this.state.amount} decimals={2} allowNegative={true} onValueChanged={value => this.updateAmount(value)} autoFocus />
                </FormField>

                <FormField label="Description" help="Par exemple: transfert de solde, correction, etc.">
                    <TextInput value={this.state.description} onValueChanged={value => this.setState({ description: value })} />
                </FormField>

                {this.state.error ? <ErrorMessage>{this.state.error}</ErrorMessage> : null}

                <p>
                    Nouveau solde: <Amount amount={this.state.newBalance} />
                </p>

                <Button type="submit">Enregistrer</Button>
                <Link to="/balances"><Button>Annuler</Button></Link>
            </form>
        );
    }

    updateAmount = value => {
        if (value === "") {
            value = 0;
        }

        const newState = {
            amount: value
        };

        if (typeof value === "number") {
            newState.newBalance = this.props.previousBalance + value;
        }

        this.setState(newState);
    }

    onSubmit = () => {
        if (this.state.amount == 0) {
            this.setState({
                error: "Le montant de l'ajustement doit être différent de zéro"
            });
            return;
        }
        if (this.state.description == "") {
            this.setState({
                error: "Veuillez entrer une description"
            });
            return;
        }

        this.setState({ error: null });
        this.props.onSubmit({ amount: this.state.amount, description: this.state.description });
    }
}

export default Adjustment;