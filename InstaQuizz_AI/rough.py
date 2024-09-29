# from SimilarityFinder import SimilarityFinder
from QuestionGenerator import QuestionGenerator
#
# # similarityFinder = SimilarityFinder()
# # simlar_sentences = ["Cream", "cream"]
# # print(similarityFinder.compute_similarity("Cream", "cream"))
# # print(similarityFinder.find_unique_sentences(sentences=simlar_sentences))
questionGenerator = QuestionGenerator()
print(questionGenerator.compute_chunk_size(67, 8))
