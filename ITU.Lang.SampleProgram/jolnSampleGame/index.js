import dict from './dict';

const minute = 60*1000;

const targetContainer = document.querySelector("#targetContainer");
const inputContainer = document.querySelector("#inputContainer");
const textInput = document.querySelector("#textInput");
const timer = document.querySelector("#timer");
const wpm = document.querySelector("#wpm");
const accuracy = document.querySelector("#accuracy");

function makeText(len = 20) {
    let buf = [];

    for(let i = 0; i < len; i++) {
        const pos = Math.floor(Math.random() * dict.length - 1);
        buf.push({
            text: dict[pos],
            correct: false,
            completedAt: null,
        });
    }

    return buf;
}

const start = new Date();
let words = makeText();
let currentWord = 0;
let hasStarted = false;

function isCorrect({text, correct}, i) {
    if(i > currentWord) {
        return true;
    }

    if(i < currentWord) {
        return correct;
    }

    const currentInp = textInput.value;

    return text.startsWith(currentInp);
}

function renderText() {
    const formattedWords = words.map(({text, correct}, i) => {
        if(i === currentWord) {
            return renderCurrentText(text);
        }

        const wordIsCorrect = isCorrect({text, correct}, i);
        const classes = [
            'py-2 px-1',
            correct ? 'text-green-600' : wordIsCorrect ? 'text-gray-800' : 'text-red-500',
        ];
        return `<span class="${classes.join(" ").trim()}">${text}</span>`;
    });

    targetContainer.innerHTML = formattedWords.join(" ");
    requestAnimationFrame(() => {
        targetContainer.querySelector(`span:nth-of-type(${currentWord + 1})`).scrollIntoView();
    });
}

function renderCurrentText(text) {
    const currentWordText = textInput.value;
    let hasIncorrectLetter = false;
    const letters = text.split('').map((letter, j) => {
        let letterClass = "";
        if(j < currentWordText.length) {
            const isCorrect = letter === currentWordText[j];
            if(!isCorrect) {
                hasIncorrectLetter = true;
            }
            letterClass = [
                `font-bold`,
                !isCorrect ? 'text-red-500' : ''
            ].join(' ');
        }
        return `<span class="${letterClass}">${letter}</span>`;
    });

    return `<span class="py-2 px-1 ${hasIncorrectLetter ? 'bg-red-200' : 'bg-gray-200'}">${letters.join("")}</span>`;
}

function renderFocus() {
    const activeClass = 'bg-gray-200';
    const inactiveClass = 'bg-gray-100';

    if(document.activeElement.parentNode === inputContainer) {
        inputContainer.classList.remove(inactiveClass);
        inputContainer.classList.add(activeClass);
    }
    else {
        inputContainer.classList.remove(activeClass);
        inputContainer.classList.add(inactiveClass);
    }
}

function renderStats() {
    if(!hasStarted) {
        return;
    }

    renderTimeStats();
    renderAccuracy();
}

function renderTimeStats() {
    const now = new Date();
    const diff = new Date(now - start);

    timer.textContent = diff.toLocaleTimeString("en-gb", {
       minute: "2-digit", second: "2-digit",
    });

    const completedWords = words.filter(({completedAt, correct}, i) => correct && i <= currentWord && completedAt !== null);
    const minuteWords = completedWords.filter(({ completedAt }) => now - completedAt < minute);

    wpm.textContent = minuteWords.length;
}

function renderAccuracy() {
    const attemptedWords = words.filter((_, i) => i <= currentWord);
    const completedWords = words.filter(({correct}) => correct);

    const attemptedWordCount = attemptedWords.length;
    // Assume curent word is correct
    const completedWordsCount = completedWords.length + 1;

    if(attemptedWordCount >= 1) {
        accuracy.textContent = Math.round((completedWordsCount / attemptedWordCount) * 100);
    }
}

renderText();
renderFocus();
renderStats();

textInput.addEventListener('keyup', (evt) => {

    if(!hasStarted) {
        setInterval(renderStats, 1000);
        hasStarted = true;
    }

    const val = evt.target.value;
    const { text } = words[currentWord];

    words[currentWord].correct = val === text + " ";

    if(evt.key === " ") {
        currentWord++;
        evt.target.value = "";
        words[currentWord].completedAt = new Date();
    }

    if(words.length - currentWord <= 10) {
        words.push(...makeText());
    }

    renderText();
    renderStats();
});

textInput.addEventListener('focus', renderFocus);

textInput.addEventListener('blur', renderFocus);

inputContainer.addEventListener('click', () => textInput.focus());
