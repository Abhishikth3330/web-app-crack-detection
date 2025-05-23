from flask import Flask, request, jsonify
from flask_cors import CORS
import numpy as np
from PIL import Image
import io
import tensorflow as tf

# Initialize Flask app
app = Flask(__name__)
CORS(app)

# Load model (update path as needed)
model = tf.keras.models.load_model("final_model.h5")

# Define image size (update based on your model)
IMG_SIZE = (150, 150)  # Example: for ResNet or MobileNet

# Preprocessing function
def preprocess_image(image_bytes):
    image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    image = image.resize(IMG_SIZE)
    image_array = np.array(image) / 255.0  # Normalize
    image_array = np.expand_dims(image_array, axis=0)  # Add batch dim
    return image_array

# Define prediction route
@app.route('/predict', methods=['POST'])
def predict():
    if 'file' not in request.files:
        return jsonify({"error": "No file provided"}), 400

    file = request.files['file']
    if file.filename == '':
        return jsonify({"error": "Empty filename"}), 400

    try:
        image_data = file.read()
        processed_image = preprocess_image(image_data)

        prediction = model.predict(processed_image)[0][0]  # Assuming binary output
        label = "Positive" if prediction > 0.5 else "Negative"
        confidence = float(prediction if prediction > 0.5 else 1 - prediction)

        return jsonify({
            "prediction": label,
            "confidence": round(confidence, 4)
        })

    except Exception as e:
        return jsonify({"error": str(e)}), 500

# Run the app
if __name__ == '__main__':
    app.run(debug=True)
