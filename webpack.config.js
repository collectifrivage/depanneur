const CleanWebpackPlugin = require("clean-webpack-plugin");

const distFolder = __dirname + "/Depanneur.App/wwwroot/bundles";

module.exports = function(_, {mode}) {
    return {
        entry: {
            app: "./Depanneur.App/Frontend/index.js"
        },
        output: {
            filename: "[name].js",
            path: distFolder
        },
        optimization: {
            splitChunks: {
                chunks: "all"
            }
        },
        module: {
            rules: [
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: ["babel-loader"]
                }
            ]
        },
        plugins: [
            new CleanWebpackPlugin([distFolder])
        ],
        devtool: mode === "production" ? "source-map" : "inline-source-map"
    }
};