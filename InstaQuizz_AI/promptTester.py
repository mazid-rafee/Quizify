from transformers import RobertaTokenizer, RobertaForMaskedLM
import torch

# Load pre-trained RoBERTa tokenizer and model
tokenizer = RobertaTokenizer.from_pretrained('deepset/roberta-large-squad2')
model = RobertaForMaskedLM.from_pretrained('deepset/roberta-large-squad2')


# Function to predict masked words in a sentence using RoBERTa
def predict_masked_words(text, top_k=5):
    # Tokenize the input and mask the blank
    input_ids = tokenizer.encode(text, return_tensors='pt')

    # Find the positions of [MASK] tokens
    mask_token_index = torch.where(input_ids == tokenizer.mask_token_id)[1]

    # Forward pass through the model
    with torch.no_grad():
        output = model(input_ids)

    # Get the logits for the masked positions
    logits = output.logits

    # Get the top k token predictions for each masked token
    mask_token_logits = logits[0, mask_token_index, :]
    top_k_tokens = torch.topk(mask_token_logits, top_k, dim=1).indices.tolist()

    # Decode the predicted tokens
    predicted_words = [tokenizer.decode([token]).strip() for token in top_k_tokens[0]]
    return predicted_words


# Example sentence with a [MASK] token
text = "Given that Python was invented in the late 1980s by Guido van Rossum at Centrum Wiskunde & Informatica (CWI) in the Netherlands as a successor to the ABC programming language, which was inspired by SETL, capable of exception handling and interfacing with the Amoeba operating system, the answer to the question what is the name of the company that invented Python is <mask>."

# Predict the masked words
predicted_words = predict_masked_words(text)
print(f"Predicted words for the masked token: {predicted_words}")
