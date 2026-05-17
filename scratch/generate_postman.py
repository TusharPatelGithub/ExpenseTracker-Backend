import json
import uuid

collection = {
    "info": {
        "name": "SpendSmart Microservices API",
        "description": "Postman Collection for SpendSmart Backend Testing",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "variable": [
        {"key": "auth_url", "value": "http://localhost:5260", "type": "string"},
        {"key": "category_url", "value": "http://localhost:5026", "type": "string"},
        {"key": "expense_url", "value": "http://localhost:5227", "type": "string"},
        {"key": "income_url", "value": "http://localhost:5257", "type": "string"},
        {"key": "budget_url", "value": "http://localhost:5138", "type": "string"},
        {"key": "notification_url", "value": "http://localhost:5267", "type": "string"},
        {"key": "report_url", "value": "http://localhost:5191", "type": "string"},
        {"key": "token", "value": "", "type": "string"}
    ],
    "item": []
}

def create_request(name, method, url_var, path, body=None, auth=True):
    req = {
        "name": name,
        "request": {
            "method": method,
            "header": [
                {"key": "Content-Type", "value": "application/json"}
            ],
            "url": {
                "raw": f"{{{{{url_var}}}}}/{path}",
                "host": [f"{{{{{url_var}}}}}"] ,
                "path": path.split("/")
            }
        },
        "response": []
    }
    
    if auth:
        req["request"]["auth"] = {
            "type": "bearer",
            "bearer": [
                {"key": "token", "value": "{{token}}", "type": "string"}
            ]
        }
        
    if body:
        req["request"]["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, indent=4)
        }
        
    return req

# 1. Auth API
auth_items = [
    create_request("Register", "POST", "auth_url", "api/users/register", {
        "fullName": "Test User",
        "email": "test@example.com",
        "password": "Password123!",
        "currency": "INR"
    }, auth=False),
    create_request("Login", "POST", "auth_url", "api/users/login", {
        "email": "test@example.com",
        "password": "Password123!"
    }, auth=False),
    create_request("Get Profile", "GET", "auth_url", "api/users/profile"),
    create_request("Admin - Get All Users", "GET", "auth_url", "api/users/admin/users"),
    create_request("Admin - Analytics", "GET", "auth_url", "api/admin/analytics")
]
collection["item"].append({"name": "Auth API", "item": auth_items})

# 2. Category API
category_items = [
    create_request("Seed Default Categories", "POST", "category_url", "api/categories/seed"),
    create_request("Get User Categories", "GET", "category_url", "api/categories/user"),
    create_request("Create Category", "POST", "category_url", "api/categories", {
        "name": "Groceries",
        "icon": "shopping-cart",
        "color": "#FF5733",
        "type": "EXPENSE"
    })
]
collection["item"].append({"name": "Category API", "item": category_items})

# 3. Expense API
expense_items = [
    create_request("Add Expense", "POST", "expense_url", "api/expenses", {
        "categoryId": 1,
        "amount": 500,
        "currency": "INR",
        "description": "Weekly groceries",
        "date": "2026-05-13T10:00:00Z",
        "paymentMode": "CARD"
    }),
    create_request("Get User Expenses", "GET", "expense_url", "api/expenses/user"),
    create_request("Get Total Expense", "GET", "expense_url", "api/expenses/total")
]
collection["item"].append({"name": "Expense API", "item": expense_items})

# 4. Income API
income_items = [
    create_request("Add Income", "POST", "income_url", "api/incomes", {
        "source": "SALARY",
        "amount": 50000,
        "currency": "INR",
        "description": "Monthly Salary",
        "date": "2026-05-01T10:00:00Z"
    }),
    create_request("Get User Incomes", "GET", "income_url", "api/incomes/user"),
    create_request("Get Total Income", "GET", "income_url", "api/incomes/total")
]
collection["item"].append({"name": "Income API", "item": income_items})

# 5. Budget API
budget_items = [
    create_request("Create Budget", "POST", "budget_url", "api/budgets", {
        "name": "Monthly Food Budget",
        "limitAmount": 10000,
        "currency": "INR",
        "period": "MONTHLY",
        "startDate": "2026-05-01T00:00:00Z",
        "endDate": "2026-05-31T23:59:59Z"
    }),
    create_request("Get User Budgets", "GET", "budget_url", "api/budgets/user"),
    create_request("Get Budget Utilization", "GET", "budget_url", "api/budgets/utilization")
]
collection["item"].append({"name": "Budget API", "item": budget_items})

# 6. Notification API
notification_items = [
    create_request("Get Unread Notifications", "GET", "notification_url", "api/notifications/unread"),
    create_request("Get User Notifications", "GET", "notification_url", "api/notifications/user"),
    create_request("Mark All As Read", "PUT", "notification_url", "api/notifications/mark-all-read")
]
collection["item"].append({"name": "Notification API", "item": notification_items})

# 7. Report API
report_items = [
    create_request("Get Monthly Summary", "GET", "report_url", "api/reports/monthly?month=5&year=2026"),
    create_request("Get Category Breakdown", "GET", "report_url", "api/reports/category-breakdown?start=2026-05-01T00:00:00Z&end=2026-05-31T23:59:59Z"),
    create_request("Generate PDF Report", "POST", "report_url", "api/reports/generate-pdf", {
        "reportType": "MONTHLY",
        "parameters": {
            "month": "5",
            "year": "2026"
        }
    })
]
collection["item"].append({"name": "Report API", "item": report_items})

with open("SpendSmart_Postman_Collection.json", "w", encoding="utf-8") as f:
    json.dump(collection, f, indent=2)

print("Collection created successfully as SpendSmart_Postman_Collection.json")
