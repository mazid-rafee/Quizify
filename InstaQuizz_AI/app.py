from flask import Flask, request, jsonify

from AnswerGenerator import AnswerGenerator
from InputProcessor import InputProcessor
from QualityAnalyzer import QualityAnalyzer
from QuestionAnswerSet import QuestionAnswerSet
from QuestionGenerator import QuestionGenerator
from SimilarityFinder import SimilarityFinder
from operator import attrgetter
from JsonHandler import JsonHandler

# Assuming load_model loads your ML model and predict_text uses it to process text

app = Flask(__name__)


@app.route('/generate_quiz', methods=['POST'])
def generate_quiz():
    try:
        # Parse the JSON request body
        data = request.get_json()
        text = data['text']
        text = InputProcessor.clear_references(text)


        questionGenerator = QuestionGenerator()
        similarityFinder = SimilarityFinder()
        answerGenerator = AnswerGenerator()

        requiredQuestions = 5
        questions = questionGenerator.generate_questions(text, requiredQuestions)
        questionSet = []
        for content, questionList in questions.items():
            questionAnswerSet = QuestionAnswerSet()
            questionAnswerSet.question, score = similarityFinder.find_ideal_sentence(questionList)
            questionAnswerSet.context = content
            questionAnswerSet.score = QualityAnalyzer.compute_quality(questionAnswerSet.question)
            questionSet.append(questionAnswerSet)
        questionSet = sorted(questionSet, key=attrgetter('score'), reverse=True)[:requiredQuestions]
        jsonCollection = []
        for qaSet in questionSet:
            qaSet.correct_answer = answerGenerator.get_correct_answer(qaSet.question, qaSet.context)
            qaSet.options = answerGenerator.generate_options(qaSet.context, qaSet.question, qaSet.correct_answer, num_distractors=4)
            jsonFormat = JsonHandler.convert_question_to_json(qaSet)
            jsonCollection.append(jsonFormat)

        


        quizes = JsonHandler.convert_json_list_to_single_str(jsonCollection)       
        # Return the prediction as a JSON response
        return quizes

    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=5000)
