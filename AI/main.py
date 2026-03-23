from fastapi import FastAPI
import joblib
import numpy as np

# Khởi tạo app
app = FastAPI()

# Load model
model = joblib.load("decision_tree_model.pkl")

# Test API
@app.get("/")
def home():
    return {"message": "AI API is running 🚀"}

# API dự đoán
@app.post("/predict")
def predict(data: dict):
    try:
        # Lấy dữ liệu từ request
        score = data["Test_Score"]
        attendance = data["Attendance"]
        hours = data["Study_Hours"]

        # Convert thành input model
        X = np.array([[score, attendance, hours]])

        # Predict xác suất rớt
        p_fail = model.predict_proba(X)[0][1]

        return {
            "p_fail": float(p_fail)
        }

    except Exception as e:
        return {"error": str(e)}