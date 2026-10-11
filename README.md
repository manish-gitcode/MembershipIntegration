# Membership Integration – CRM → Legacy Mapping

Takes a CRM member registration JSON, validates it, and maps it to the legacy membership system payload.

Exposed as an HTTP-triggered Azure Function: `POST /api/map-registration`

## Structure

```
MembershipIntegration.sln
├── MembershipIntegration/          # Azure Function (.NET 10, isolated worker)
│   ├── Models/
│   ├── Services/RegistrationMapper.cs
│   ├── MapRegistrationFunction.cs
│   └── Program.cs
└── MembershipIntegration.Tests/
```

## Mapping

| CRM                | Legacy                     |
|--------------------|----------------------------|
| firstName          | member.given_name          |
| lastName           | member.family_name         |
| dateOfBirth        | member.dob (dd/MM/yyyy)    |
| email              | member.contact.email       |
| membershipType     | member.plan_code (S/F/C)   |
| (always)           | member.source = "MILKYWAY" |

Plan codes: Single → S, Family → F, Couple → C

Extra fields in the input are ignored.

## Validation

Returns 400 if:
- required fields missing/empty (firstName, lastName, dateOfBirth, email, membershipType)
- email looks invalid
- membershipType not Single/Family/Couple
- dateOfBirth can't be parsed

## Run locally

```bash
dotnet build
dotnet test
func start
```

Then POST to `http://localhost:7071/api/map-registration`

## Notes for production

If this were wired to a real outbound call you'd want:
- auth (client credentials / managed identity)
- HttpClientFactory + retries/timeouts (Polly or .NET resilience)
- App Insights for logging
- queue/dead-letter for transient failures
