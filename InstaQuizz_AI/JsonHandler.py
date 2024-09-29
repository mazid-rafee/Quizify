import json
class JsonHandler:
    @staticmethod
    def convert_to_json_array(lst):
        return json.dumps(lst)

    @staticmethod
    def convert_question_to_json(questionAnswerSet):
        qaJson = {
            "question": questionAnswerSet.question,
            "correct_answer": questionAnswerSet.correct_answer,
            "options": questionAnswerSet.options
        }
        return json.dumps(qaJson)

    @staticmethod
    def convert_json_list_to_single_str(json_strings):
        jsonObjects = [json.loads(json_string) for json_string in json_strings]
        jsonArray = JsonHandler.convert_to_json_array(jsonObjects)
        return jsonArray



