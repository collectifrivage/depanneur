import gql from "graphql-tag";
import React from "react";
import { Query } from "react-apollo";

import store from "store";
import BalancesList from "../components/balances-list";
import { Checkbox } from "../components/forms";
import Loading from "../components/loading-indicator";
import OptionGroup from "../components/option-group";


const balancesQuery = gql`
    query AllBalances($includeDeleted: Boolean) {
        users(includeDeleted: $includeDeleted) {
            id
            name
            balance
            isDeleted
        }
    }
`;

class Balances extends React.Component {
    constructor() {
        super();
        this.state = {
            balances: null,
            sortOption: store.get("dep.balances.sort") || "name",
            hideZero: store.get("dep.balances.hideZero") === true,
            hideDeleted: store.get("dep.balances.hideDeleted") === true
        };

        this.sortOptions = [
            {key: "name", label: "Nom"},
            {key: "balance", label: "Solde"}
        ];
    }

    render() {
        return (
            <div>
                <h1>Soldes</h1>
                
                <div style={{display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", marginBottom: 10}}>
                    <OptionGroup label="Tri :" 
                                 options={this.sortOptions} 
                                 selectedOption={this.state.sortOption} 
                                 onOptionSelected={o => this.setSortOption(o)} />
                    <div>
                        <Checkbox label="Masquer les soldes à zéro" checked={this.state.hideZero} onChecked={c => this.setHideZero(c)} /><br/>
                        <Checkbox label="Masquer les utilisateurs supprimés" checked={this.state.hideDeleted} onChecked={c => this.setHideDeleted(c)} />
                    </div>
                </div>

                
                <Query query={balancesQuery} variables={{includeDeleted: !this.state.hideDeleted}} fetchPolicy="cache-and-network">
                    {
                        ({error, data}) => {
                            if (error) return <div>Erreur</div>;
                            if (!data.users) return <Loading />;

                            return <BalancesList balances={this.getBalances(data.users)} />
                        }
                    }
                </Query>
            </div>
        );
    }

    setSortOption(option) {
        store.set("dep.balances.sort", option);

        this.setState({
            sortOption: option
        });
    }

    sortBalances(balances, option) {
        const copy = balances.slice();

        copy.sort(function(a, b) {
            function compareNames() {
                if (a.name < b.name) return -1;
                if (a.name > b.name) return 1;
                return 0;
            }

            switch (option) {
                case "balance": 
                    return (b.balance - a.balance) || compareNames();
                default:
                    return compareNames();
            }
        });

        return copy;
    }

    setHideZero(hide) {
        store.set("dep.balances.hideZero", hide);
        this.setState({
            hideZero: hide
        });
    }

    setHideDeleted(hide) {
        store.set("dep.balances.hideDeleted", hide);
        this.setState({
            hideDeleted: hide
        });
    }

    getBalances(balances) {
        const filtered = balances.filter(b => this.state.hideZero === false || b.balance !== 0)
        return this.sortBalances(filtered, this.state.sortOption);
    }
}

export default Balances;