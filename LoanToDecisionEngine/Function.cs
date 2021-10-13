using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
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

            //Step machine API URL
            //var url = "https://g9yh14f7ve.execute-api.ap-south-1.amazonaws.com/Authorizeddev/execution";
            var url = Environment.GetEnvironmentVariable("StepFunctionURL");

            StepCallBody stepbody = new StepCallBody();
            stepbody.input = json;
            //stepbody.name = "MyExecution";
            stepbody.name = Environment.GetEnvironmentVariable("StepBodyName");
            //stepbody.stateMachineArn = "arn:aws:states:ap-south-1:052987743965:stateMachine:PBLoanProcessOrchestration";
            stepbody.stateMachineArn = Environment.GetEnvironmentVariable("StateMachineArn");

            string stepjson = JsonConvert.SerializeObject(stepbody);

            StringContent bodydata = new StringContent(stepjson, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            //Need creditor Authorization access token
            client.DefaultRequestHeaders.Add("Authorization", "eyJraWQiOiJXSlpET21BQ0RuS3FHVVhZU2VFXC9pU0J5Y2VRS0xLNlJXdmFiK2pXcDFyWT0iLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiIyODEwZmM1OS1lZjNiLTRjNjctYmY5Ni0xMzEzZjExYjdiMzUiLCJldmVudF9pZCI6Ijk0Yzk0MzViLTM2ZmItNDhmOS05MWYwLTY0ODRhNzQ5NzA0ZiIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4gcGhvbmUgb3BlbmlkIHByb2ZpbGUgZW1haWwiLCJhdXRoX3RpbWUiOjE2MzQwOTc2NTcsImlzcyI6Imh0dHBzOlwvXC9jb2duaXRvLWlkcC5hcC1zb3V0aC0xLmFtYXpvbmF3cy5jb21cL2FwLXNvdXRoLTFfbzVYdlNEOW44IiwiZXhwIjoxNjM0MTg0MDU3LCJpYXQiOjE2MzQwOTc2NTcsInZlcnNpb24iOjIsImp0aSI6ImJmY2Q0NDE2LTcyN2QtNDM3ZC05ZDhiLTdmYWI3OTg4OGRlNCIsImNsaWVudF9pZCI6IjRjYTA0NTVqMzZxdXI2ZW11ZjRvOGNmbGtjIiwidXNlcm5hbWUiOiJjcmVkaXRvcjEifQ.XCc6fioShxcOoeuHHYmaH0efyIp9MqERmaA8QxfkGWdNDLaSgIvQNttcVmEZZspegxFHWbp1jJRI6zrztiYnw4O0uIAL_6lnoBRzouIZth164QAoFmJV3HogerCj8Ot0_P904UbuPEKSNjDvAwaTlvJ2ZoiNGC5-WOioTI7rwCn-keS5imY8imKaXzecVBu6zKpkkgsvzwGSzjzCe4mplVMJvuWZrB_bzNPb18_DcPtHCenqUn3koRoH7tgT3LZkgZt6-Hne4nos2wwpmxQRS429kSJd_hPCWWTtmw4NOMpqL6F5H53Hc2sMSl0z-VDO11YpQ0j2r83pGR3hMPFsDg");
            var response = await client.PostAsync(url, bodydata);

            await Task.CompletedTask;
        }
    }
}
