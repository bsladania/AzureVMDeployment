//Credits: 3BUXO4P24no

using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;

namespace AzureVMDeployment
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Enter user name for Windows VM: ");
            string username = Console.ReadLine();

            Console.WriteLine("Enter password for Windows VM: ");
            string password = Console.ReadLine();

            Console.WriteLine("Confirm password: ");

            while(password != Console.ReadLine())
            {
                Console.WriteLine("Alert: Password does not match!");
                Console.WriteLine("Confirm password: ");
            }

            Console.WriteLine("Windows Virtual Machine Devlopment has been started...");

            //Get Azure credentials
            var credentials = SdkContext.AzureCredentialsFactory
                                            .FromFile("./azureauth.properties");
            
            //Azure Authentication
            var azure = Azure.Configure()
                                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                                .Authenticate(credentials)
                                .WithDefaultSubscription();

            //Create required variables
            var groupName = "az204rg";
            var vmName = "az204demovm";
            var location = Region.CanadaCentral;
            var vNetName = "az204net";
            var vNetAddress = "10.10.0.0/16";
            var subnetName = "az204subnet";
            var subnetAddress = "10.10.0.0/24";
            var nicName = "az204nic";

            var resourceGroup = azure.ResourceGroups.Define(groupName)
                                        .WithRegion(location)
                                        .Create();
            Console.WriteLine($"Resource group [{groupName}] has been created for {vmName} VM.");

            var network = azure.Networks.Define(vNetName)
                                    .WithRegion(location)
                                    .WithExistingResourceGroup(groupName)
                                    .WithAddressSpace(vNetAddress)
                                    .WithSubnet(subnetName,subnetAddress)
                                    .Create();

            Console.WriteLine($"Virtual Network [{vNetName}] has been created for {vmName} VM.");

            var publicIPAddress = azure.PublicIPAddresses.Define("samplePublicIP")  
                                        .WithRegion(location)  
                                        .WithExistingResourceGroup(groupName)  
                                        .WithDynamicIP()  
                                        .Create();
            
            Console.WriteLine($"Public IP Address has been created for {vmName} VM.");

            var networkInterfaceCard = azure.NetworkInterfaces.Define(nicName)
                                                .WithRegion(location)
                                                .WithExistingResourceGroup(groupName)
                                                .WithExistingPrimaryNetwork(network)
                                                .WithSubnet(subnetName)
                                                .WithPrimaryPrivateIPAddressDynamic()
                                                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                                                .Create();

            Console.WriteLine($"Network Interface Card [{nicName}] has been created for {vmName} VM.");

            azure.VirtualMachines.Define(vmName)
                                    .WithRegion(location)
                                    .WithExistingResourceGroup(groupName)
                                    .WithExistingPrimaryNetworkInterface(networkInterfaceCard)
                                    .WithLatestWindowsImage("MicrosoftWindowsServer","WindowsServer","2019-Datacenter")
                                    .WithAdminUsername(username)
                                    .WithAdminPassword(password)
                                    .Create();

            Console.WriteLine($"Congratulations! [{vmName}] has been created.");

        }
    }
}
