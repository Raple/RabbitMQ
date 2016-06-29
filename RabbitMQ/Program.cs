using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ
{
    class Program
    {
        private static readonly ConnectionFactory rabbitMqFactory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest", VirtualHost = "/" };
        const string ExchangeName = "test.exchange";
        const string QueueName = "test.queue";

        static void Main(string[] args)
        {             
            using (IConnection conn = rabbitMqFactory.CreateConnection())
            using (IModel channel = conn.CreateModel())
            {
                channel.ExchangeDeclare(ExchangeName, "direct", durable:true, autoDelete:false, arguments:null); //定义交换方式
                channel.QueueDeclare(QueueName, durable:true, exclusive:false, autoDelete:false,arguments:null); //定义消息队列
                channel.QueueBind(QueueName, ExchangeName, routingKey: QueueName); //把消息队列绑定到某个交换方式上


                //持久化消息到队列
                var props = channel.CreateBasicProperties();
                props.Persistent=true;
                string read = "Hello, World!";
                read=Console.ReadLine();
                var msgBody = Encoding.UTF8.GetBytes(read);
                channel.BasicPublish(ExchangeName, routingKey: QueueName, basicProperties: props, body: msgBody);


                //消费队列中的消息
                //NoAck:true 告诉RabbitMQ立即从队列中删除消息，
                //另一个非常受欢迎的方式是从队列中删除已经确认接收的消息，可以通过单独调用BasicAck 进行确认
                //以下用的是第二种
                BasicGetResult msgResponse = channel.BasicGet(QueueName, noAck: false); 
                var msgBodyReturn = Encoding.UTF8.GetString(msgResponse.Body);
                Console.WriteLine(msgBodyReturn);
                //确定之后手动调用删除队列中的消息
                channel.BasicAck(msgResponse.DeliveryTag, multiple: false);

            }




        }
    }
}
