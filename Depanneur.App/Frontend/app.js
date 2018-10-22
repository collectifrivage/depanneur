import React from "react";
import Radium from "radium";
import { Switch, Route } from "react-router";

import commonStyles from "./styles";
import Header from "./components/header";
import Notifications from "./components/notifications";

import Home from "./pages/home";
import Balances from "./pages/balances";
import Payment from "./pages/payment";
import Adjustment from "./pages/adjustment";
import MyTransactions from "./pages/my-transactions";
import UserTransactions from "./pages/user-transactions";
import Products from "./pages/products";
import AddProduct from "./pages/add-product";
import EditProduct from "./pages/edit-product";
import ProductPurchases from "./pages/product-purchases";
import Users from "./pages/users";
import AddUser from "./pages/add-user";
import Subscription from "./pages/subscription";

const App = Radium(props => {
    return (
        <div>
            <Header />
            <Notifications />
            <div style={[commonStyles.block, {paddingBottom: 100}]}>
            
                <Switch>
                    <Route exact path="/" component={Home} />
                    <Route exact path="/balances" component={Balances} />
                    <Route exact path="/payment/:userid" component={Payment} />
                    <Route exact path="/adjustment/:userid" component={Adjustment} />
                    <Route exact path="/transactions" component={MyTransactions} />
                    <Route exact path="/transactions/:userid" component={UserTransactions} />
                    <Route exact path="/products" component={Products} />
                    <Route exact path="/products/add" component={AddProduct} />
                    <Route exact path="/products/:productid" component={EditProduct} />
                    <Route exact path="/products/:productid/purchases" component={ProductPurchases} />
                    <Route exact path="/users" component={Users} />
                    <Route exact path="/users/add" component={AddUser} />
                    <Route exact path="/subscription/:productid" component={Subscription} />
                </Switch>

            </div>
            <div style={[commonStyles.block, styles.footer]}>
                <a href="http://www.sigmund.ca" target="_blank">
                    <img src="/sigmund.svg" style={styles.signature} />
                </a>
            </div>
        </div>
    );
});

const styles = {
    footer: {
        textAlign: "right"
    },
    signature: {
        width: 100,
        opacity: 0.2,
        transition: "all 1s",

        ":hover": {
            width: 120,
            opacity: 0.8
        }
    }
}

export default App;