from transformers import T5ForConditionalGeneration, T5Tokenizer
import nltk
nltk.download('punkt')
from nltk.tokenize import sent_tokenize

class QuestionGenerator:
    def __init__(self):
        # Load pre-trained T5 model and tokenizer for question generation
        self.tokenizer = T5Tokenizer.from_pretrained('google/flan-t5-base')
        self.model = T5ForConditionalGeneration.from_pretrained('google/flan-t5-base')
        self.chunk_sizes = [64, 128, 256, 512]

    def chunk_text(self, text, chunk_size):
        sentences = sent_tokenize(text)
        chunks = []

        total_len = 0
        chunk = ""
        for sentence in sentences:
            if total_len + len(sentence) <= chunk_size:
                chunk = chunk + sentence + " "
                total_len += len(sentence)
            else:
                if len(chunk.strip()) > 0:
                    chunks.append(chunk.strip())
                total_len = 0
                chunk = ""

        return chunks

    def compute_chunk_size(self, text_len, required_questions):
        for chunk_size in self.chunk_sizes:
            possible_chunks = text_len // chunk_size
            # print("Possible chunks using chunk size of "+str(chunk_size)+ ": "+str(possible_chunks))
            if possible_chunks < required_questions:
                if chunk_size == self.chunk_sizes[0]:
                    return chunk_size
                else:
                    return chunk_size - (chunk_size / 2)
        return self.chunk_sizes[len(self.chunk_sizes) - 1]

    def generate_questions(self, text, required_questions):
        chunk_size = self.compute_chunk_size(len(text), required_questions)
        chunks = self.chunk_text(text, chunk_size)

        questionsByChunk = {}
        for chunk in chunks:
            # Prefix for the input text, instructing the model to generate questions
            input_text = f"generate questions from the given text: {chunk}"

            # Tokenize the input text
            input_ids = self.tokenizer.encode(input_text, return_tensors="pt")

            # Generate questions using the T5 model (increase num_beams to match or exceed num_return_sequences)
            outputs = self.model.generate(input_ids,
                                          max_length=chunk_size,
                                          num_beams=3,
                                          num_return_sequences=3,
                                          early_stopping=True,
                                          top_p=0.6,  # Nucleus sampling for diversity
                                          top_k=100,  # Limits sampling to top k tokens
                                          temperature=1.0,  # Controls randomness in generation
                                          )

            # Decode the generated questions and return them as a list

            questionsByChunk[chunk] = [self.tokenizer.decode(output, skip_special_tokens=True) for output in outputs] 
            if len(questionsByChunk) >= required_questions + 2:
                break

        return questionsByChunk



