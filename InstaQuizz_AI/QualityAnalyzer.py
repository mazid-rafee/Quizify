import textstat

class QualityAnalyzer:
    @staticmethod
    def compute_quality(sentence):
        fk_grade = textstat.flesch_kincaid_grade(sentence)
        gunning_fog = textstat.gunning_fog(sentence)
        smog_index = textstat.smog_index(sentence)

        return (gunning_fog + fk_grade + smog_index) / 3