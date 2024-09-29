

import random
from transformers import RobertaTokenizer, RobertaForMaskedLM, RobertaForQuestionAnswering
from transformers import BertTokenizer, BertForQuestionAnswering
import torch
import re

from SimilarityFinder import SimilarityFinder


class AnswerGenerator:
    def __init__(self):
        qaModelName = "bert-large-cased-whole-word-masking-finetuned-squad"
        self.qaTokenizer = BertTokenizer.from_pretrained(qaModelName)
        self.qaModel = BertForQuestionAnswering.from_pretrained(qaModelName)
        # Load pre-trained RoBERTa tokenizer and model
        self.robertaTokenizer = RobertaTokenizer.from_pretrained('roberta-base')
        self.robertaModel = RobertaForMaskedLM.from_pretrained('roberta-base')
        self.similarityFinder = SimilarityFinder()
        # self.answeringModel = RobertaForQuestionAnswering.from_pretrained("deepset/roberta-base-squad2")
        self.pattern = r"[^\w\s.,]"

    def get_correct_answer(self, question, context):
        # Tokenize the input (context and question)
        inputs = self.qaTokenizer.encode_plus(question, context, return_tensors="pt")

        # Get the model outputs (start and end logits for answer span)
        with torch.no_grad():
            outputs = self.qaModel(**inputs)

        # Get the most likely start and end positions for the answer
        answer_start = torch.argmax(outputs.start_logits)
        answer_end = torch.argmax(outputs.end_logits) + 1

        # Convert the token IDs back into a string (the answer)
        correct_answer = self.qaTokenizer.convert_tokens_to_string(
            self.qaTokenizer.convert_ids_to_tokens(inputs["input_ids"][0][answer_start:answer_end]))

        return correct_answer

    def prompt_generator(self, context, question, correct_answer):
        if len(context) > 0 and context[len(context) - 1] == '.':
            context = context[:-1]
        prompt = "Based on the text: " + context + ", " + question + " The correct answer is: "+correct_answer+". A similar answer is: <mask>."
        return prompt

    def generate_potential_distractors(self, prompt, top_k=10):
        # Tokenize the input and mask the blank
        input_ids = self.robertaTokenizer.encode(prompt, return_tensors='pt')

        # Find the positions of [MASK] tokens
        mask_token_index = torch.where(input_ids == self.robertaTokenizer.mask_token_id)[1]

        # Forward pass through the model
        with torch.no_grad():
            output = self.robertaModel(input_ids)

        # Get the logits for the masked positions
        logits = output.logits

        # Get the top k token predictions for each masked token
        mask_token_logits = logits[0, mask_token_index, :]
        top_k_tokens = torch.topk(mask_token_logits, top_k, dim=1).indices.tolist()

        # Decode the predicted tokens
        predicted_words = [self.robertaTokenizer.decode([token]).strip() for token in top_k_tokens[0]]
        return predicted_words

    # def remove_unrealistic_answers(self, answers):
    #     similarities = {}
    #     for targetAnswer in answers:
    #         total_similarity = 0
    #         for answer in answers:
    #             total_similarity += self.similarityFinder.compute_similarity(targetAnswer, answer)
    #         average_similarity = total_similarity / (len(answers) - 1)
    #         similarities[targetAnswer] = average_similarity
    #
    #     similarities = dict(sorted(similarities.items(), key=lambda item: item[1]))
    #     return list(similarities.keys())

    def eliminate_similar_answers(self, answers):
        answers = self.similarityFinder.find_unique_sentences(answers)
        return answers

    def clean_answers(self, answers, correct_answer):
        cleaned_answers = []
        for idx in range(len(answers)):
            answers[idx] = answers[idx].strip()
            if str(answers[idx][0]).isalpha():
                answers[idx] = answers[idx][0].upper() + answers[idx][1:]
            punctations = re.findall(self.pattern, answers[idx])
            if len(punctations) == 0 or answers[idx] == correct_answer:
                cleaned_answers.append(answers[idx])
        return cleaned_answers




    # Function to generate distractors from WordNet
    def generate_options(self, context, question, correct_answer, num_distractors):
        prompt = self.prompt_generator(context, question, correct_answer)
        potential_distractors = self.generate_potential_distractors(prompt, num_distractors + 20)
        distractors = self.eliminate_similar_answers([correct_answer] + potential_distractors)
        distractors = self.clean_answers(distractors, correct_answer)
        distractors = distractors[:num_distractors]
        random.shuffle(distractors)
        return distractors
