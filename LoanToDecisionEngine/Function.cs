using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LoanToDecisionEngine
{
    public class Function
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");

            ProcessMessages pmobj = new ProcessMessages();
            pmobj = JsonConvert.DeserializeObject<ProcessMessages>(message.Body);
            pmobj.LoanApplication_Status = 5; //Loan_Status 5 i.e. Approved
            pmobj.LoanApplication_BankerComment = "Approved";

            string json = JsonConvert.SerializeObject(pmobj);
            string StepBodyName = Environment.GetEnvironmentVariable("StepBodyName");
            string StateMachineArn = Environment.GetEnvironmentVariable("StateMachineArn");
            string ExternalAccessKey = Environment.GetEnvironmentVariable("ExternalAccessKey");
            string ExternalSecreteKey = Environment.GetEnvironmentVariable("ExternalSecreteKey");

            var awsCredentials = new BasicAWSCredentials(ExternalAccessKey, ExternalSecreteKey);
            var awsclient = new Amazon.StepFunctions.AmazonStepFunctionsClient(awsCreden‌​tials, Amazon.RegionEndpoint.APSouth1);

            Amazon.StepFunctions.Model.StartExecutionRequest req = new Amazon.StepFunctions.Model.StartExecutionRequest();

            req.Input = json;
            req.Name = StepBodyName;
            req.StateMachineArn = StateMachineArn;
             
            var aws_response = await awsclient.StartExecutionAsync(req);




            await Task.CompletedTask;
        }
    }
}
