using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Extensions;

namespace OracleExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoredProcedureController : ControllerBase
    {
        private readonly ILogger<StoredProcedureController> _logger;

        public StoredProcedureController(ILogger<StoredProcedureController> logger)
        {
            _logger = logger;
        }


        [HttpGet("/cursor_example")]
        public async Task<IActionResult> cursor_example(int depno)
        {
            StreamReader reader = new StreamReader( HttpContext.Request.Body );
            string s = await reader.ReadToEndAsync();
            string jsonReq = JsonSerializer.Serialize(HttpContext.Request.Headers); 
            _logger.LogInformation(DateTime.Now + "\n" + HttpContext.Request.GetDisplayUrl() + "\n" + s + jsonReq);
            //const string connString = @"User Id=ocrm;Password=YourOCRM123;Data Source=127.0.0.1:1521/DB12C;";
            //const string connString = @"Data Source=DB12C=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521)))(CONNECT_DATA=(SID=DB12C)));User Id=ocrm;Password=YourOCRM123";
            const string connString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=db12c)));User Id=ocrm;Password=YourOCRM123";
            

            const string storedProcName = "cursor_example";
            using (OracleConnection conn = new OracleConnection(connString))
            using (OracleCommand cmd = conn.CreateCommand())
            {
                try {
                    conn.Open();
                    OracleGlobalization info = conn.GetSessionInfo();
                    info.TimeZone = "Europe/Minsk";
                    conn.SetSessionInfo(info);
                    cmd.CommandText = storedProcName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("i_depno", OracleDbType.Int32, depno, ParameterDirection.Input);
                    OracleParameter o_result = cmd.Parameters.Add("o_result", OracleDbType.Varchar2,  ParameterDirection.ReturnValue);
                    OracleParameter o_cur = cmd.Parameters.Add("o_cur",OracleDbType.RefCursor, ParameterDirection.ReturnValue );
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    string json = JsonSerializer.Serialize(ds); 
                    return Content(json);


                }
                catch (Exception e) {
                    _logger.LogError(e.Message);
                    return Content(e.Message);

                }
            }

            
        }
    }
}