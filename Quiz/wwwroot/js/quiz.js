const formElement = document.getElementById("inputForm");
const wordInputElement = document.getElementById("wordInput");
const outputElement = document.getElementById("found-words");
const timerElement = document.getElementById("quizTimer");
const remainingWordsElement = document.getElementById("remainingWords");
const totalWordsElement = document.getElementById("totalWords");
const stopQuizElement = document.getElementById("stopQuiz");

const [quizId] = document.location.href.split("/").slice(-1);
const apiURL = "/aspdotnet/moment4/api/quiz";
/**
 *
 * @param {Array<string>} words
 * @param {Set<string>} remainingWords
 */
const update = (words, remainingWords) => {
    const lyrics = [];
    words.forEach(word => {
        if (!remainingWords.has(word.toLowerCase())) {
            lyrics.push(word)
        } else {
            lyrics.push(`<span style="width:${word.length}ch" class="missing"></span>`);
        }
    })
    outputElement.innerHTML = lyrics.join(" ");
    remainingWordsElement.innerHTML = `${remainingWords.size}`;
}

/**
 *
 * @param {string} quizId
 */
const redirectToResult = (quizId) => {
    document.location.href = `${document.location.protocol}//${document.location.href.split("/").filter(part => part).slice(1, -2).join("/")}/results/${quizId}/`
}

/**
 *
 * @param {Object<string, any>} obj
 */
const toFormData = (obj) => {
    const formData = new FormData();
    for (const [key, value] of Object.entries(obj)) {
        formData.append(key, value);
    }
    return formData;
}


/**
 *
 * @param {number} intervalId
 * @param {Array<string>} words
 * @param {Set<string>} remainingWords
 * @param {Object} quizData
 * @param {number} diff
 */
const endQuiz = async (intervalId, words, remainingWords, quizData, diff) => {
    clearInterval(intervalId);
    const body = {
        "PercentCompleted": 1 - remainingWords.size / new Set(words.map(word => word.toLowerCase())).size,
        "Time": quizData.timeLimitSec - diff,
        "QuizId": quizId,
        "MissingWords": JSON.stringify(Array.from(remainingWords))
    }
    console.log(body);
    let response = await fetch(`${apiURL}/result/`, {
        method: "post",
        body: toFormData(body)
    }).then(async data => data.json());
    redirectToResult(response.id);
}


window.addEventListener("load", async () => {
    const quizData = await fetch(`${apiURL}/${quizId}/`).then(data => data.json());
    const words = quizData.lyric.split(" ").filter(word => word);
    const remainingWords = new Set(words.map(word => word.toLowerCase()));
    totalWordsElement.innerHTML = `${remainingWords.size}`;
    update(words, remainingWords);
    let started = false;
    let intervalId = -1;
    let start = null;
    formElement.addEventListener("submit", async (e) => {
        e.preventDefault();
        if (!started) {
            stopQuizElement.disabled = false;
            start = new Date();
            intervalId = setInterval(async () => {
                const diff = Math.round((new Date() - start) / 1000);
                if (diff < quizData.timeLimitSec) {
                    const seconds = (quizData.timeLimitSec - diff) % 60;
                    timerElement.innerHTML = `${Math.floor((quizData.timeLimitSec - diff) / 60)}:${seconds > 9 ? seconds : `0${seconds}`}`
                } else {
                    await endQuiz(intervalId, words, remainingWords, quizData, diff);
                }
            }, 499);

            stopQuizElement.addEventListener("click", async () => endQuiz(
                intervalId, words, remainingWords, quizData, Math.round((new Date() - start) / 1000)
            ));
            started = true;
        }
        for (const word of wordInputElement.value.toLowerCase().split(" ")) {
            if (remainingWords.has(word)) {
                remainingWords.delete(word);
                if (remainingWords.size === 0) {
                    await endQuiz(
                        intervalId, words, remainingWords, quizData, Math.round((new Date() - start) / 1000)
                    );
                }
                update(words, remainingWords);
            }
            wordInputElement.value = "";
        }
    });
})
