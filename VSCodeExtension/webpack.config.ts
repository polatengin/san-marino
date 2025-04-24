import path from "path";
import webpack from "webpack";

const outputPath = path.resolve(__dirname, "out");

const extensionConfig: webpack.Configuration = {
  target: "node",
  entry: "./extension.ts",
  devtool: "source-map",
  output: {
    path: outputPath,
    filename: "extension.js",
    libraryTarget: "commonjs2",
    devtoolModuleFilenameTemplate: "file:///[absolute-resource-path]",
  },
  externals: {
    vscode: "commonjs vscode",
    "applicationinsights-native-metrics": "commonjs applicationinsights-native-metrics",
    "@opentelemetry/tracing": "commonjs @opentelemetry/tracing",
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        loader: "esbuild-loader",
        options: {
          loader: "ts",
          target: "es2019",
        },
        exclude: [/node_modules/, /panes\/deploy\/app/, /visualizer\/app/, /test/],
      },
    ],
  },
  resolve: {
    extensions: [".ts", ".js"],
    conditionNames: ["import", "require"],
  },
};

module.exports = (_env: unknown, argv: { mode: string }) => {
  return [extensionConfig];
};
