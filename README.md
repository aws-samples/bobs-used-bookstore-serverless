## My Project

TODO: Fill this README out!

cdk deploy AuthenticationStack --require-approval=never --app "dotnet run --project cdk/src/AuthenticationStack/AuthenticationStack.csproj"
cdk deploy BookInventoryServiceStack --require-approval=never --app "dotnet run --project cdk/src/BookInventoryApiStack/BookInventoryApiStack.csproj"

```
aws cognito-idp admin-create-user --user-pool-id <USER_POOL_ID> --username john@example.com --user-attributes Name="given_name",Value="john" Name="family_name",Value="smith"
```

```
aws cognito-idp admin-set-user-password --user-pool-id <USER_POOL_ID> --username john@example.com --password "<PASSWORD>" --permanent
```

```
aws cognito-idp admin-initiate-auth --cli-input-json file://auth.json
```

**auth.json**
```json
{
    "UserPoolId": "<USER_POOl_ID>",
    "ClientId": "<CLIENT_ID>",
    "AuthFlow": "ADMIN_NO_SRP_AUTH",
    "AuthParameters": {
        "USERNAME": "john@example.com",
        "PASSWORD": "<PASSWORD>"
    }
}
```

Be sure to:

* Change the title in this README
* Edit your repository description on GitHub

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.

