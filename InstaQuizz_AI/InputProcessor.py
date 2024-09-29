import re
class InputProcessor:
    @staticmethod
    def clear_references(text):
        # Define the regular expression pattern to match [x] where x is a number
        pattern = r'\[\d+\]'

        # Use re.sub() to replace all instances of [x] with an empty string
        text = re.sub(pattern, '', text)

        text = text.strip()

        return text
