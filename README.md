## Bob's Used Books Serverless


## Overview 
Bob's Used BookStore serverless is a serverless version of the [Bob's Used Books Sample Application](https://github.com/aws-samples/bobs-used-bookstore-sample).

## Prerequisites
To run and debug the application locally you need the following:
* The [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* A modern IDE, for example [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [JetBrains Rider](https://www.jetbrains.com/rider/)

To deploy the application to AWS you need the following:
* An AWS IAM User with an attached _AdministratorAccess_ policy
* The [AWS Cloud Development Kit (CDK)](https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html)
* [Bootstrap](https://docs.aws.amazon.com/cdk/v2/guide/bootstrapping.html) your AWS environment for the AWS CDK by executing `cdk bootstrap` in a terminal window

## Getting started
TODO

## Deployment
Bookstore application can be deployed to AWS via the CDK's command-line tooling.

`cdk deploy --all`

## How to test

The following microservices are used while building BookStore serverless:

- [Book Inventory Api](/src/BookInventoryApi/README.md)


## Deleting the resources

When you have completed working with the sample applications we recommend deleting the resources to avoid possible charges. To do this, either:

* In a terminal window navigate to the solution folder and run the command `cdk destroy --all`, or

* Navigate to the CloudFormation dashboard in the AWS Management Console and delete all Bob's Used BookStore Serverless stacks.
## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.

