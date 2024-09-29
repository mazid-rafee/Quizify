from sentence_transformers import SentenceTransformer, util
import torch
from sympy import false


class SimilarityFinder:
    def __init__(self):
        # Force CPU usage by setting the device to CPU
        self.device = torch.device("cpu")
        # Load the Sentence-BERT model
        self.model = SentenceTransformer('paraphrase-MiniLM-L6-v2', device=str(self.device))

    def get_embedding(self, text):
        # Get embeddings from the model
        return self.model.encode(text)

    # Function to compute similarity score between two questions
    def compute_similarity(self, q1, q2):
        # Force using CPU
        device = torch.device('cpu')

        # Get embeddings and move them to the CPU (ensures no MPS issues)
        q1_embedding = torch.tensor(self.get_embedding(q1)).to(device)
        q2_embedding = torch.tensor(self.get_embedding(q2)).to(device)

        # Compute cosine similarity
        cosine_sim = util.pytorch_cos_sim(q1_embedding, q2_embedding).item()

        return cosine_sim


    def find_ideal_sentence(self, sentences):
        embeddings = self.model.encode(sentences, convert_to_tensor=True)

        # Compute pairwise cosine similarities between all sentences
        similarity_matrix = util.pytorch_cos_sim(embeddings, embeddings)

        # Compute average similarity for each sentence
        avg_similarities = similarity_matrix.mean(dim=1)

        # Find the sentence with the highest average similarity
        best_index = avg_similarities.argmax().item()

        # Return the most ideal sentence and its average similarity score
        return sentences[best_index], avg_similarities[best_index].item()

    def generate_marker(self, questions):
        markers = {}
        for question in questions:
            markers[question] = True
        return markers

    def find_unique_sentences(self, sentences):
        uniqueSentences = []
        markers = self.generate_marker(sentences)
        for targetSentence in sentences:
            if markers[targetSentence] is False:
                continue

            sentenceOmitted = false
            markers[targetSentence] = False
            similarSentences = [targetSentence]
            # print("Target Question: ", targetQuestion)
            for sentence in sentences:
                sim_score = self.compute_similarity(targetSentence, sentence)
                if sim_score >= 0.7:
                    markers[sentence] = False
                    similarSentences.append(sentence)
                    sentenceOmitted = True
                    # print("Similar question: " + str(question) + "(Score: "+str(sim_score)+")")
            bestSentence, similarity = self.find_ideal_sentence(similarSentences)
            # print("Best question: "+best_question)
            if len(bestSentence) > 0:
                uniqueSentences.append(bestSentence)

            if sentenceOmitted is False:
                break

        return uniqueSentences



