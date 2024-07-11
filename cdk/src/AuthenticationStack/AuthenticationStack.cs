namespace AuthenticationStack;

using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.SSM;

using Constructs;

public record AuthenticationProps(string Postfix);

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
            $"BookStoreUserPool{authProps.Postfix}",
            new UserPoolProps
            {
                UserPoolName = $"book-store-users{authProps.Postfix}",
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
                CustomAttributes = new Dictionary<string, ICustomAttribute>()
                {
                    { "user_id", new StringAttribute(new StringAttributeProps { Mutable = false }) } // Setup Guid on setting up user id
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
            $"BookStoreClient{authProps.Postfix}",
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
                    }).WithCustomAttributes(["user_id"]),
                WriteAttributes = new ClientAttributes().WithStandardAttributes(
                    new StandardAttributesMask()
                    {
                        GivenName = true,
                        FamilyName = true,
                        Email = true
                    }).WithCustomAttributes(["user_id"])
            });

        var cfnUserPoolAdminGroup = new CfnUserPoolGroup(this, $"BookStoreAdminGroup{authProps.Postfix}", new CfnUserPoolGroupProps {
            UserPoolId = userPool.UserPoolId,
            Description = "Bookstore Admin group",
            GroupName = "Admin",
            Precedence = 123,
        });
        var cfnUserPoolCustomerGroup = new CfnUserPoolGroup(this, $"BookStoreCustomerGroup{authProps.Postfix}", new CfnUserPoolGroupProps {
            UserPoolId = userPool.UserPoolId,
            Description = "Bookstore Buyer",
            GroupName = "Customer",
            Precedence = 124,
        });
        
        var userPoolParameter = new StringParameter(
            this,
            $"UserPoolParameter{authProps.Postfix}",
            new StringParameterProps()
            {
                ParameterName = $"/bookstore/authentication/user-pool-id{authProps.Postfix}",
                StringValue = userPool.UserPoolArn
            });

        var userPoolClientParameter = new StringParameter(
            this,
            $"UserPoolClientParameter{authProps.Postfix}",
            new StringParameterProps()
            {
                ParameterName = $"/bookstore/authentication/user-pool-client-id{authProps.Postfix}",
                StringValue = userPoolClient.UserPoolClientId
            });

        var userPoolOutput = new CfnOutput(
            this,
            $"UserPoolId{authProps.Postfix}",
            new CfnOutputProps()
            {
                Value = userPool.UserPoolId,
                ExportName = $"UserPoolId{authProps.Postfix}"
            });

        var clientIdOutput = new CfnOutput(
            this,
            $"ClientId{authProps.Postfix}",
            new CfnOutputProps()
            {
                Value = userPoolClient.UserPoolClientId,
                ExportName = $"ClientId{authProps.Postfix}"
            });
    }
}