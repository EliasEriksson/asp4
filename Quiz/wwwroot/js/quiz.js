const form = document.getElementById("inputForm");
const wordInput = document.getElementById("wordInput");
const formSubmitButton = document.getElementById("inputFormSubmitButton");
const output = document.getElementById("found-words");
const [quizId] = document.location.href.split("/").slice(-1);
const timer = document.getElementById("quizTimer");
const startQuiz = document.getElementById("startQuiz");

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
    output.innerHTML = lyrics.join(" ");
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


window.addEventListener("load", async () => {
    const quizData = await fetch(`${apiURL}/${quizId}/`).then(data => data.json());
    console.log(quizData)
    const words = quizData.lyric.split(" ");
    const remainingWords = new Set(words.map(word => word.toLowerCase()))
    update(words, remainingWords);

    startQuiz.addEventListener("click", async () => {
        startQuiz.disabled = true;
        wordInput.disabled = false;
        formSubmitButton.disabled = false;
        let start = new Date();
        const intervalId = setInterval(async () => {
            const diff = Math.round((new Date() - start) / 1000);
            if (diff < quizData.timeLimitSec) {
                const seconds = (quizData.timeLimitSec - diff) % 60;
                timer.innerHTML = `${Math.floor((quizData.timeLimitSec - diff) / 60)}:${seconds > 9 ? seconds : `0${seconds}`}`
            } else {
                clearInterval(intervalId);
                let response = await fetch(`${apiURL}/result/`, {
                    method: "post",
                    body: toFormData({
                        "PercentCompleted": 1 - remainingWords.size / new Set(words).size,
                        "Time": diff,
                        "QuizId": quizId,
                        "MissingWords": JSON.stringify(Array.from(remainingWords))
                    })
                }).then(async data => {
                    console.log(data)
                    return data.json();
                });
                console.log(response);
                redirectToResult(response.id);
            }
        }, 499)
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (remainingWords.has(wordInput.value.toLowerCase())) {
                remainingWords.delete(wordInput.value);
                if (remainingWords.size === 0) {
                    const response = await fetch(`${apiURL}/result/`, {
                        method: "post",
                        body: toFormData({
                            "PercentCompleted": 1,
                            "Time": Math.round((start - new Date()) / 1000),
                            "QuizId": quizId,
                            "MissingWords": JSON.stringify(Array.from(remainingWords))
                        })
                    }).then(data => data.json());
                    redirectToResult(response.id);
                }
                update(words, remainingWords);
            }
            wordInput.value = "";
        })
    })
})