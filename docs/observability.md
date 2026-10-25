# Observability & Monitoring

`dotnet-web-foundation` provides modern telemetry, health checks, and structured logging ready for enterprise monitoring platforms (e.g., Prometheus, Grafana, Datadog, AWS CloudWatch, Azure Monitor).

---

## 1. Structured Logging (Serilog)

- **Sink Configuration:** Logs are structured in JSON format with console formatting for development and structured enrichment in production.
- **Log Enrichment:** Enriched with `SourceContext`, `CorrelationId`, and `ThreadId`.
- **Sensible Filtering:** Default information level for application logic with suppression of verbose framework SQL/HTTP telemetry.

---

## 2. OpenTelemetry Tracing & Metrics

The application registers the standard OpenTelemetry .NET SDK with automatic instrumentation:

- **ASP.NET Core Instrumentation:** Traces incoming HTTP server requests (health checks filtered out from noisy tracing).
- **HTTP Client Instrumentation:** Traces outgoing downstream HTTP client calls.
- **Runtime Instrumentation:** Reports GC collections, CPU utilization, thread pool allocations, and memory footprint.
- **OTLP Exporter:** Emits traces and metrics over gRPC / HTTP to an OpenTelemetry collector via `OpenTelemetry:OtlpEndpoint`.

---

## 3. Health Endpoints

Two standard health endpoints are exposed for container orchestrators (Kubernetes, AWS ECS, Docker Compose):

| Endpoint | Path | Purpose |
| :--- | :--- | :--- |
| **Liveness** | `/health/live` | Returns HTTP 200 if the ASP.NET Core process is running and responding. |
| **Readiness** | `/health/ready` | Verifies the database connection is healthy and responsive before routing traffic. |
