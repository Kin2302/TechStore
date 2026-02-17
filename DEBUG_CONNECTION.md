# ?? Debug Script - Ki?m tra l?i k?t n?i AI

## ? Các fix ?ã áp d?ng:

### 1?? **??i Model ID**
```csharp
// Tr??c: gemini-2.5-flash (có th? không t?n t?i)
// Sau: gemini-1.5-flash (stable)
```

### 2?? **T?ng Timeout**
```csharp
// Tr??c: 30 seconds
// Sau: 60 seconds
```

### 3?? **Thêm Detailed Logging**
```csharp
_logger?.LogInformation("=== ChatAsync Start ===");
_logger?.LogInformation("Sending request to Gemini API...");
_logger?.LogError(ex, "L?i k?t n?i Gemini API: {Message}", ex.Message);
```

### 4?? **Thêm Health Check Endpoint**
```
GET /api/Chat/health
```

---

## ?? Testing Steps:

### **Step 1: Test Health Check**
```bash
GET https://localhost:7001/api/Chat/health
```

**Expected Response:**
```json
{
  "status": "healthy",
  "service": "ChatController",
  "timestamp": "2026-01-23T10:30:00Z"
}
```

---

### **Step 2: Test Chat v?i logging**

#### **Postman Request:**
```bash
POST https://localhost:7001/api/Chat/chat
Content-Type: application/json

{
  "message": "Hello"
}
```

#### **Ki?m tra Console Log:**
```
=== ChatAsync Start ===
User Message: Hello
Sending request to Gemini API...
AI Response received: 45 ký t?
=== ChatAsync Success ===
```

---

### **Step 3: N?u v?n l?i, ki?m tra:**

#### **A. Ki?m tra API Key h?p l?:**
```bash
# Test API key tr?c ti?p v?i curl
curl "https://generativelanguage.googleapis.com/v1beta/models?key=AIzaSyA6N7jye9_TJEIdQy7O_msZCFbJ6DT1slI"
```

**Expected:** Tr? v? danh sách models

---

#### **B. Ki?m tra Network:**
```bash
# Test connection
ping generativelanguage.googleapis.com
```

---

#### **C. Ki?m tra Firewall/Proxy:**
```bash
# N?u có corporate proxy, thêm vào Program.cs:
builder.Services.AddHttpClient("GeminiClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        Proxy = new WebProxy("http://your-proxy:port"),
        UseProxy = true
    });
```

---

## ?? Common Errors & Solutions:

### **Error 1: "Invalid API Key"**
```
?? L?i k?t n?i AI: The API key is invalid
```

**Solution:**
1. Vào [Google AI Studio](https://makersuite.google.com/app/apikey)
2. T?o API key m?i
3. C?p nh?t trong `appsettings.json`:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_NEW_API_KEY"
  }
}
```

---

### **Error 2: "Model not found"**
```
?? L?i k?t n?i AI: Model 'gemini-2.5-flash' not found
```

**Solution:** ? ?ã fix b?ng cách ??i sang `gemini-1.5-flash`

---

### **Error 3: "Request timeout"**
```
?? H? th?ng AI ph?n h?i ch?m, vui lòng th? l?i
```

**Solution:** ? ?ã t?ng timeout lên 60s

---

### **Error 4: "Rate limit exceeded"**
```
?? L?i k?t n?i AI: Resource has been exhausted (e.g. check quota)
```

**Solution:**
- ??i 1 phút r?i th? l?i
- Ho?c nâng c?p quota trong Google Cloud Console

---

### **Error 5: "Network unreachable"**
```
?? Không th? k?t n?i v?i AI. Vui lòng ki?m tra k?t n?i m?ng
```

**Solution:**
- Ki?m tra internet
- T?t VPN n?u có
- Ki?m tra firewall

---

## ??? Debug Commands:

### **1. Xem logs trong Visual Studio:**
```
View ? Output ? Show output from: Debug
```

### **2. Xem logs trong terminal:**
```bash
cd D:\DotNet\TechStore\WebApp
dotnet run --verbosity detailed
```

### **3. Test tr?c ti?p v?i Semantic Kernel:**
```csharp
// Thêm vào ChatController ?? test
[HttpPost("test")]
public async Task<IActionResult> TestGemini()
{
    try
    {
        var result = await _geminiService.ChatAsync("Hello");
        return Ok(new { success = true, result });
    }
    catch (Exception ex)
    {
        return Ok(new { success = false, error = ex.ToString() });
    }
}
```

---

## ?? Checklist Debug:

- [ ] **API Key ?úng?** (Test b?ng curl)
- [ ] **Model ID ?úng?** (gemini-1.5-flash)
- [ ] **Network OK?** (Ping ???c Google API)
- [ ] **Firewall/Proxy?** (Không block Google API)
- [ ] **Quota?** (Ch?a v??t gi?i h?n)
- [ ] **Logs?** (Xem l?i c? th? trong console)

---

## ?? Next Steps:

### **N?u v?n l?i:**

1. **Copy toàn b? error message t? log**
2. **Screenshot error t? Postman**
3. **Chia s? ?? debug sâu h?n**

---

## ?? Quick Test Scenarios:

### **Scenario 1: Simple Message**
```json
POST /api/Chat/chat
{
  "message": "Hello"
}
```

### **Scenario 2: Search Product**
```json
{
  "message": "Tìm Arduino"
}
```

### **Scenario 3: Add to Cart**
```json
{
  "message": "Thêm Arduino Uno vào gi?"
}
```

---

## ? Expected Behavior:

Sau khi apply fixes:
- ? Response time: 2-5 seconds (l?n ??u)
- ? Response time: 0.5-2 seconds (các l?n sau)
- ? Logs chi ti?t trong console
- ? Error message rõ ràng n?u có

---

**Author:** TechStore Team  
**Date:** 2026-01-23  
**Branch:** AIFeature
