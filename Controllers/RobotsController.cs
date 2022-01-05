using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RobotsController : Controller
    {
        [HttpGet]
        [Route("api/robots/index")]
        public object Index()
        {
            return null;
        }

        [HttpGet]
        [Route("api/robots/closest")]
        public ClosestRobot Closest(LoadToBePickedUp loadToBePickedUp)
        {
            ClosestRobot closestRobot = new ClosestRobot();
            List<ClosestRobot> closeRobots = new List<ClosestRobot>();
            List<RobotStatus> robotStatuses;
            double distance;
            HttpClient client = new HttpClient();
            string url = "https://60c8ed887dafc90017ffbd56.mockapi.io/robots";
            string response;
            string body;

            try
            {
                // For some reason - it was not able to deserialize the request body into the object LoadToBePickedUp.  So I had to do it manually here
                if ((loadToBePickedUp == null) || string.IsNullOrWhiteSpace(loadToBePickedUp.loadId))
                {
                    Request.EnableBuffering();
                    Request.Body.Seek(0, SeekOrigin.Begin);

                    using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
                    {
                        body = stream.ReadToEndAsync().Result;
                    }
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        loadToBePickedUp = JsonConvert.DeserializeObject<LoadToBePickedUp>(body);
                    }
                }

                Debug.WriteLine(Request.ContentLength);
                if ((loadToBePickedUp == null) || string.IsNullOrWhiteSpace(loadToBePickedUp.loadId))
                    return null;
                response = client.GetStringAsync(url).Result;
                robotStatuses = JsonConvert.DeserializeObject<List<RobotStatus>>(response);
                closestRobot.distanceToGoal = double.MaxValue;
                foreach (RobotStatus robotStatus in robotStatuses)
                {
                    distance = Math.Sqrt(Math.Pow(robotStatus.x - loadToBePickedUp.x, 2) + Math.Pow(robotStatus.y - loadToBePickedUp.y, 2));
                    if ((closeRobots.Count == 0) && (distance > 10) && (closestRobot.distanceToGoal > distance))
                    {
                        closestRobot.distanceToGoal = distance;
                        closestRobot.robotId = robotStatus.robotId;
                        closestRobot.batteryLevel = robotStatus.batteryLevel;
                    }
                    else if (distance <= 10)
                    {
                        closestRobot = new ClosestRobot();
                        closestRobot.distanceToGoal = distance;
                        closestRobot.robotId = robotStatus.robotId;
                        closestRobot.batteryLevel = robotStatus.batteryLevel;
                        closeRobots.Add(closestRobot);
                    }
                }
                if (closeRobots.Count > 0)
                {
                    closestRobot = (from r in closeRobots orderby r.batteryLevel descending select r).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: " + ex.Message);
            }
            return closestRobot;
        }
    }
}
