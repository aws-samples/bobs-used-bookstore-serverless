namespace AuthenticationStack;

using Amazon.CDK;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;

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
        Topic snsTopic = new Topic(
            this,
            "testSNS",
            new TopicProps());
        Queue sqs = new Queue(
            this,
            "testQueue",
            new QueueProps());
        snsTopic.AddSubscription(new SqsSubscription(sqs, new SqsSubscriptionProps()));
    }
}