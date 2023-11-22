namespace AuthenticationStack;

using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.SSM;

using Constructs;

public record AuthenticationProps();

public class AuthenticationStack : Stack
{
    internal AuthenticationStack(
        Construct scope,
        string id,
        AuthenticationProps authProps,
        IStackProps props = null) : base(
        scope,
        id,
        props)
    {
        var userPool = new UserPool(
            this,
            "BookStoreUserPool",
            new UserPoolProps
            {
                UserPoolName = "book-store-users",
                SelfSignUpEnabled = true,
                SignInAliases = new SignInAliases
                {
                    Email = true
                },
                AutoVerify = new AutoVerifiedAttrs
                {
                    Email = true
                },
                StandardAttributes = new StandardAttributes
                {
                    GivenName = new StandardAttribute
                    {
                        Required = true
                    },
                    FamilyName = new StandardAttribute
                    {
                        Required = true
                    }
                },
                PasswordPolicy = new PasswordPolicy
                {
                    MinLength = 6,
                    RequireDigits = true,
                    RequireLowercase = true,
                    RequireSymbols = false,
                    RequireUppercase = false
                },
                AccountRecovery = AccountRecovery.EMAIL_ONLY,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

        var userPoolClient = userPool.AddClient(
            "BookStoreClient",
            new UserPoolClientOptions
            {
                UserPoolClientName = "api-login",
                AuthFlows = new AuthFlow()
                {
                    AdminUserPassword = true,
                    Custom = true,
                    UserSrp = true
                },
                SupportedIdentityProviders = new[]
                {
                    UserPoolClientIdentityProvider.COGNITO,
                },
                ReadAttributes = new ClientAttributes().WithStandardAttributes(
                    new StandardAttributesMask()
                    {
                        GivenName = true,
                        FamilyName = true,
                        Email = true,
                        EmailVerified = true
                    }),
                WriteAttributes = new ClientAttributes().WithStandardAttributes(
                    new StandardAttributesMask()
                    {
                        GivenName = true,
                        FamilyName = true,
                        Email = true
                    })
            });

        var userPoolParameter = new StringParameter(
            this,
            "UserPoolParameter",
            new StringParameterProps()
            {
                ParameterName = "/bookstore/authentication/user-pool-id",
                StringValue = userPool.UserPoolArn
            });

        var userPoolClientParameter = new StringParameter(
            this,
            "UserPoolClientParameter",
            new StringParameterProps()
            {
                ParameterName = "/bookstore/authentication/user-pool-client-id",
                StringValue = userPoolClient.UserPoolClientId
            });

        var userPoolOutput = new CfnOutput(
            this,
            "UserPoolId",
            new CfnOutputProps()
            {
                Value = userPool.UserPoolId,
                ExportName = "UserPoolId"
            });

        var clientIdOutput = new CfnOutput(
            this,
            "ClientId",
            new CfnOutputProps()
            {
                Value = userPoolClient.UserPoolClientId,
                ExportName = "ClientId"
            });
    }
}