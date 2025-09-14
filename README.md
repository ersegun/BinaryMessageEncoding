# Binary Message Encoding API

A **.NET 8 Web API** for encoding and decoding structured messages into a compact **binary format**.  
Built with clean architecture principles, validation, testing, and Docker support for easy integration into larger systems.

## Features
- Encode/decode messages for a signaling protocol.  
- Validation for payload size, header count, header name/value size.  
- ASCII-only enforcement for headers.  
- RFC7807-compliant error handling (Problem Details for HTTP APIs).  
- Custom `MessageEncodingException` for codec-specific errors.  
- Swagger documentation for easy testing.  
- NUnit unit tests for core functionality.  
- Docker-ready for containerized deployments.  

## Design Choices

### Encoding Scheme
The binary encoding scheme is as follows:

```
[header_count:1 byte]
For each header:
   [key_length:2 bytes][key_bytes]
   [value_length:2 bytes][value_bytes]
[payload_length:4 bytes]
[payload_bytes]
```

### Validation & Error Handling
- **MessageValidator:**  
  Ensures semantic correctness (ASCII-only headers, max lengths, max headers, payload size).  
- **MessageCodec:**  
  Ensures structural correctness (stream corruption, length mismatches, truncated data).  
- **MessageEncodingException:**  
  Provides a domain-specific exception type for codec errors, preserving the original stack trace via inner exceptions.

## Quick Start

Run locally:
```bash
dotnet restore
dotnet run --project BinaryMessageEncodingAPI
```

Run with Docker:
```bash
docker build -t binary-message-api -f BinaryMessageEncodingAPI/Dockerfile .
docker run -p 8080:8080 binary-message-api
```

Run tests:
```bash
dotnet test
```

## Deployment & Operations

**Packaging: How would you package your IMessageCodec implementation to be usedby a larger C#/.NET application? (e.g., as a NuGet package, a source-code module, astandalone containerized service, etc.). Briefly explain your reasoning.**

I would package the IMessageCodec in two ways: first, as a lightweight NuGet library containing the core codec, validator, and dependency injection extensions so .NET applications can consume it directly with maximum performance and minimal dependencies; and second, as a containerized service like a Docker image exposing REST API  endpoints, and that makes the codec accessible to external or non-.NET clients in a language-agnostic, secure, and scalable way. This dual approach ensures internal .NET teams can integrate the codec seamlessly via NuGet, while external partners or cross-language applications can rely on the containerized API, with both distributions following clear semantic versioning to manage compatibility over time.


**Deployment: Imagine your codec is used by a "Signaling Service" that routes real-timemessages. How would you recommend deploying this service on AWS for high availability andscalability? Name the key AWS services you would use (e.g., EKS, ECS, Lambda, EC2 AutoScaling Groups) and briefly justify your choice.**

For deploying the message codec Service on AWS with high availability and scalability, I would containerize the codec service and run it on either Amazon EKS (Elastic Kubernetes Service) or Amazon ECS Fargate (serverless containers), depending on the team’s Kubernetes maturity. The service would be distributed across multiple Availability Zones through a Network Load Balancer (NLB) to ensure low-latency routing and failover. Auto Scaling (EKS Horizontal Pod Autoscaler or ECS Service Autoscaling) would handle traffic spikes, while a multi-AZ deployment guarantees resilience against single-AZ failures. This setup provides both the elasticity needed for real-time workloads and the reliability required for always-on signaling. The codec service would scale automatically with ECS Service Autoscaling to handle traffic spikes, and the Signaling Service would always reach it through a stable endpoint, ensuring the codec stays reliable and flexible under load.


**Monitoring: Once deployed, how would you monitor the health and performance of your encoding/decoding logic in production? What are the 2-3 most critical metrics you wouldwant to see on a dashboard?**

To monitor the health and performance of the codec in production, I would integrate it with Amazon CloudWatch (or another tool like Datadog) to track the metrics. The 3 most critical metrics I’d want on a dashboard are:

- Latency (average and p95/p99) → to ensure encoding/decoding stays fast enough for real-time use cases.
- Error rate (percentage of failed or malformed messages) → to quickly detect issues with message format, validation, or integration.
- Throughput / request volume (requests per second) → to understand load patterns and confirm that autoscaling is keeping up with demand.

These metrics together would give a clear view of whether the codec is healthy, performant, and keeping up with real-time traffic requirements.


**CI/CD: Briefly outline the key stages you would implement in a CI/CD pipeline to automatically build, test, and deploy this IMessageCodec component.**

For a CI/CD pipeline, I would set up four key stages:

- Build → Restore dependencies, compile the IMessageCodec project, and create artifacts or Docker images.
- Test → Run unit tests and static code analysis (SonarQube integration).
- Package → If the build passes, publish either a NuGet package (for .NET clients) or a container image (for service deployment) to a private/public registry.
- Deploy → Use GitHub Actions or AWS CodePipeline to release the new version to AWS ECS/EKS, with canary deployment or blue/green deployment.

This pipeline ensures every change is automatically validated, packaged, and delivered safely into production.
